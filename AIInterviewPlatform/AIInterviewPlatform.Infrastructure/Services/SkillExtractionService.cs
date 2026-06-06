using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.SkillExtraction;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class SkillExtractionService : ISkillExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<SkillExtractionService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public SkillExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SkillExtractionService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured");
        }
    }

    public async Task<SkillExtractionResult> ExtractSkillsFromResumeAsync(
        string resumeContent)
    {
        if (string.IsNullOrWhiteSpace(resumeContent))
        {
            _logger.LogWarning("Resume content is empty");
            return new SkillExtractionResult { Skills = [] };
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Gemini API key is not configured");
            throw new InvalidOperationException("Gemini API key is not configured");
        }

        var prompt = BuildPrompt(resumeContent);
        var response = await SendGeminiRequestAsync(prompt);
        
        return ParseGeminiResponse(response);
    }

    private string BuildPrompt(string resumeContent)
    {
        return $@"Analyze the following resume and extract ONLY the technical skills, programming languages, 
            frameworks, tools, and technologies mentioned. Return ONLY a valid JSON object 
            with this exact format - no markdown, no explanation:
            {{""skills"": [""skill1"", ""skill2"", ""skill3""]}}
            
            Resume Content:
            {resumeContent}
            
            Rules:
            - Return ONLY the JSON object
            - Use exact property name ""skills""
            - Each skill should be clean and standardized (e.g., ""C#"" not ""c#"", ""ASP.NET Core"" not ""asp.net"")
            - Remove duplicates if any
            - Include only technical skills, not soft skills (e.g., ""communication"", ""teamwork"" are not technical)";
    }

    private async Task<string> SendGeminiRequestAsync(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 800,
                responseMimeType = "text/plain"
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogDebug("Sending request to Gemini API");
            
            var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent);

            var responseBody = await response.Content.ReadAsStringAsync();
            
            _logger.LogDebug("Gemini API response: {Response}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Response}", 
                    response.StatusCode, responseBody);
                throw new InvalidOperationException($"Gemini API returned {response.StatusCode}");
            }

            return responseBody;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API");
            throw new InvalidOperationException("Failed to extract skills from resume", ex);
        }
    }

    private SkillExtractionResult ParseGeminiResponse(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array &&
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? string.Empty;
                    return ExtractSkillsFromJsonText(text);
                }
            }

            _logger.LogWarning("Unexpected Gemini response format");
            return new SkillExtractionResult { Skills = [] };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response");
            throw new InvalidOperationException("Failed to parse skill extraction response", ex);
        }
    }

    private SkillExtractionResult ExtractSkillsFromJsonText(string jsonText)
    {
        var cleanedText = jsonText.Trim();
        
        if (cleanedText.StartsWith("```json"))
        {
            cleanedText = cleanedText[7..];
        }
        else if (cleanedText.StartsWith("```"))
        {
            cleanedText = cleanedText[3..];
        }

        if (cleanedText.EndsWith("```"))
        {
            cleanedText = cleanedText[..^3];
        }

        cleanedText = cleanedText.Trim();

        var result = TryDeserialize(cleanedText);
        
        if (result == null)
        {
            result = TryExtractWithRegex(cleanedText);
        }

        return result ?? new SkillExtractionResult { Skills = [] };
    }

    private SkillExtractionResult? TryDeserialize(string jsonText)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);
            
            if (parsed.TryGetProperty("skills", out var skillsElement) &&
                skillsElement.ValueKind == JsonValueKind.Array)
            {
                var skills = new List<string>();
                
                foreach (var skill in skillsElement.EnumerateArray())
                {
                    var skillValue = skill.GetString();
                    if (!string.IsNullOrWhiteSpace(skillValue))
                    {
                        var normalized = NormalizeSkill(skillValue);
                        if (!string.IsNullOrWhiteSpace(normalized))
                        {
                            skills.Add(normalized);
                        }
                    }
                }

                return new SkillExtractionResult 
                { 
                    Skills = skills.Distinct().ToList() 
                };
            }

            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "JSON deserialization failed, trying regex extraction");
            return null;
        }
    }

    private SkillExtractionResult TryExtractWithRegex(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            text, 
            @"\{.*?""skills""\s*:\s*\[(.*?)\].*\}",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        if (match.Success)
        {
            var skillsArray = match.Groups[1].Value;
            var skillMatches = System.Text.RegularExpressions.Regex.Matches(
                skillsArray, 
                @"""([^""]+)""");

            var skills = skillMatches
                .Select(m => NormalizeSkill(m.Groups[1].Value))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            return new SkillExtractionResult { Skills = skills };
        }

        _logger.LogWarning("Could not extract skills using regex from response: {Text}", text);
        return new SkillExtractionResult { Skills = [] };
    }

    private static string NormalizeSkill(string skill)
    {
        var normalized = skill.Trim();
        
        normalized = normalized.ToUpperInvariant() switch
        {
            "C#" or "C SHARP" => "C#",
            "C++" or "C PLUS PLUS" => "C++",
            ".NET" or "DOT NET" or "DOTNET" => ".NET",
            "NODE.JS" or "NODEJS" or "NODE JS" => "Node.js",
            "JAVASCRIPT" => "JavaScript",
            "TYPESCRIPT" => "TypeScript",
            "PYTHON" => "Python",
            "JAVA" => "Java",
            "POSTGRESQL" or "POSTGRES" => "PostgreSQL",
            "MONGODB" => "MongoDB",
            "DOCKER" => "Docker",
            "KUBERNETES" or "K8S" => "Kubernetes",
            "REDIS" => "Redis",
            "RABBITMQ" => "RabbitMQ",
            "GRAPHQL" => "GraphQL",
            "REST API" or "RESTAPI" or "REST" => "REST API",
            "GIT" => "Git",
            "AWS" => "AWS",
            "AZURE" => "Azure",
            "GCP" or "GOOGLE CLOUD" => "GCP",
            "MICROSERVICES" => "Microservices",
            "SQL SERVER" or "MSSQL" => "SQL Server",
            "SQL" => "SQL",
            "HTML" => "HTML",
            "CSS" => "CSS",
            "REACT" => "React",
            "ANGULAR" => "Angular",
            "VUE" or "VUE.JS" => "Vue.js",
            "ASP.NET CORE" or "ASP.NET" or "ASPNETCORE" => "ASP.NET Core",
            "ENTITY FRAMEWORK" or "EF" => "Entity Framework",
            "LINQ" => "LINQ",
            "WEB API" => "Web API",
            "CI/CD" or "CICD" => "CI/CD",
            "AGILE" => "Agile",
            "SCRUM" => "Scrum",
            "REACTJS" or "REACT JS" => "React",
            "NEXTJS" or "NEXT JS" => "Next.js",
            "VUEJS" or "VUE JS" => "Vue.js",
            "SPRING BOOT" => "Spring Boot",
            "SPRING" => "Spring",
            "DJANGO" => "Django",
            "FLASK" => "Flask",
            "FASTAPI" or "FAST API" => "FastAPI",
            "EXPRESS" or "EXPRESS JS" => "Express.js",
            "JQUERY" => "jQuery",
            "BOOTSTRAP" => "Bootstrap",
            "TAILWIND" or "TAILWIND CSS" => "Tailwind CSS",
            "SASS" or "SCSS" => "SASS/SCSS",
            "MUI" or "MATERIAL UI" => "Material UI",
            "POSTMAN" => "Postman",
            "SWAGGER" => "Swagger",
            "JENKINS" => "Jenkins",
            "GITHUB ACTIONS" or "GITHUB_WORKFLOWS" => "GitHub Actions",
            "GITLAB CI" => "GitLab CI",
            "TERRAFORM" => "Terraform",
            "ANSIBLE" => "Ansible",
            "LINUX" => "Linux",
            "WINDOWS SERVER" => "Windows Server",
            "NGINX" => "Nginx",
            "APACHE" => "Apache",
            "JIRA" => "Jira",
            "CONFLUENCE" => "Confluence",
            "TRELLO" => "Trello",
            "SLACK" => "Slack",
            "VS CODE" or "VSC" => "VS Code",
            "VISUAL STUDIO" => "Visual Studio",
            "INTELLIJ" or "INTELLIJ IDEA" => "IntelliJ IDEA",
            "ECLIPSE" => "Eclipse",
            "ANDROID STUDIO" => "Android Studio",
            "XCODE" => "Xcode",
            "FIGMA" => "Figma",
            "ADOBE XD" => "Adobe XD",
            "SKETCH" => "Sketch",
            "ZEPLIN" => "Zeplin",
            "INVISION" => "InVision",
            "BLENDER" => "Blender",
            "UNITY" => "Unity",
            "UNREAL ENGINE" => "Unreal Engine",
            "OPENGL" => "OpenGL",
            "DIRECTX" => "DirectX",
            "VULKAN" => "Vulkan",
            "MACHINE LEARNING" or "ML" => "Machine Learning",
            "DEEP LEARNING" or "DL" => "Deep Learning",
            "NLP" or "NATURAL LANGUAGE PROCESSING" => "NLP",
            "COMPUTER VISION" => "Computer Vision",
            "NEURAL NETWORK" or "NEURAL NETWORKS" => "Neural Networks",
            "TENSORFLOW" => "TensorFlow",
            "PYTORCH" => "PyTorch",
            "KERAS" => "Keras",
            "SCIKIT LEARN" or "SKLEARN" => "Scikit-learn",
            "PANDAS" => "Pandas",
            "NUMPY" => "NumPy",
            "SCIPY" => "SciPy",
            "MATPLOTLIB" => "Matplotlib",
            "SEABORN" => "Seaborn",
            "PLOTLY" => "Plotly",
            "SPARK" => "Apache Spark",
            "HADOOP" => "Hadoop",
            "KAFKA" => "Kafka",
            "ELASTICSEARCH" => "Elasticsearch",
            "LOGSTASH" => "Logstash",
            "KIBANA" => "Kibana",
            "PROMETHEUS" => "Prometheus",
            "GRAFANA" => "Grafana",
            "DYNAMODB" => "DynamoDB",
            "FIRESTORE" => "Firestore",
            "FIREBASE" => "Firebase",
            "SUPABASE" => "Supabase",
            "PRISMA" => "Prisma",
            "SEQUELIZE" => "Sequelize",
            "TYPEORM" => "TypeORM",
            "HIBERNATE" => "Hibernate",
            "MYBATIS" => "MyBatis",
            "JPA" => "JPA",
            "MAVEN" => "Maven",
            "GRADLE" => "Gradle",
            "NPM" => "npm",
            "YARN" => "Yarn",
            "PNPM" => "pnpm",
            "WEBPACK" => "Webpack",
            "VITE" => "Vite",
            "GRUNT" => "Grunt",
            "GULP" => "Gulp",
            "ESLINT" => "ESLint",
            "PRETTIER" => "Prettier",
            "HUSKY" => "Husky",
            "LINT-STAGED" => "lint-staged",
            "JEST" => "Jest",
            "MOCHA" => "Mocha",
            "CHAI" => "Chai",
            "CYPRESS" => "Cypress",
            "PLAYWRIGHT" => "Playwright",
            "SELENIUM" => "Selenium",
            "SWAGGER UI" => "Swagger UI",
            "OPENAPI" => "OpenAPI",
            "GRPC" => "gRPC",
            "APOLLO" => "Apollo",
            "NEXUS" => "Nexus Repository",
            "ARTIFACTORY" => "Artifactory",
            "SONARQUBE" => "SonarQube",
            "STRIPE" => "Stripe",
            "PAYMENT GATEWAY" => "Payment Gateway",
            "TWILIO" => "Twilio",
            "SENDGRID" => "SendGrid",
            "AWS SES" => "AWS SES",
            "SOCKET.IO" => "Socket.IO",
            "SIGNALR" => "SignalR",
            "WEBRTC" => "WebRTC",
            "GRAPHQL SUBSCRIPTIONS" => "GraphQL Subscriptions",
            _ => normalized
        };

        return normalized;
    }
}
