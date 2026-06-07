using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using AIInterviewPlatform.Application.DTOs.AI;
using AIInterviewPlatform.Application.DTOs.SkillExtraction;
using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class SkillExtractionService : ISkillExtractionService
{
    private static readonly Dictionary<string, string> SkillNormalizationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["auditing processes"] = "Auditing",
        ["auditing process"] = "Auditing",
        ["financial data analysis"] = "Data Analysis",
        ["audit documentation preparation"] = "Audit Documentation",
        ["microsoft word professional reporting"] = "Microsoft Word",
        ["excel data analysis reporting"] = "Microsoft Excel",
        ["report compilation"] = string.Empty,
        ["error detection"] = string.Empty,
        ["bookkeeping and reporting tools"] = "Accounting"
    };

    private static readonly string[] ExcludedSkillPhrases =
    [
        "responsible for",
        "worked on",
        "assisted with",
        "supported",
        "handled",
        "performed",
        "preparation",
        "processes",
        "activities",
        "tasks",
        "achievements",
        "project",
        "report compilation",
        "error detection"
    ];

    private sealed class SkillExtractionAttemptResult
    {
        public AIServiceResult<string>? TransportResult { get; init; }
        public AIServiceResult<SkillExtractionResult>? ParsedResult { get; init; }
        public bool WasOutputTruncated { get; init; }
    }

    private static class ResumeTextSanitizer
    {
        public static string Sanitize(string resumeContent, int maxLength)
        {
            var builder = new StringBuilder(resumeContent.Length);

            foreach (var ch in resumeContent)
            {
                if (ch == '\0')
                {
                    continue;
                }

                if (char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t')
                {
                    continue;
                }

                builder.Append(ch);
            }

            var sanitized = builder.ToString()
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Replace('\t', ' ');

            sanitized = CollapseWhitespace(sanitized);

            return sanitized.Length <= maxLength
                ? sanitized
                : sanitized[..maxLength];
        }

        private static string CollapseWhitespace(string input)
        {
            var builder = new StringBuilder(input.Length);
            var previousWasSpace = false;

            foreach (var ch in input)
            {
                if (ch == '\n')
                {
                    if (builder.Length > 0 && builder[^1] == ' ')
                    {
                        builder.Length--;
                    }

                    if (builder.Length == 0 || builder[^1] != '\n')
                    {
                        builder.Append('\n');
                    }

                    previousWasSpace = false;
                    continue;
                }

                if (char.IsWhiteSpace(ch))
                {
                    if (!previousWasSpace)
                    {
                        builder.Append(' ');
                        previousWasSpace = true;
                    }

                    continue;
                }

                builder.Append(ch);
                previousWasSpace = false;
            }

            return builder.ToString().Trim();
        }
    }

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<SkillExtractionService> _logger;
    private readonly IPromptProviderFactory _promptProviderFactory;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    private const string GeminiModel = "gemini-2.5-flash";
    private const int RetryCount = 3;
    private const int MaxResumeContentLength = 4000;
    private const int DefaultMaxOutputTokens = 800;
    private const int RetryMaxOutputTokens = 2000;

    public SkillExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SkillExtractionService> logger,
        IPromptProviderFactory promptProviderFactory)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
        _promptProviderFactory = promptProviderFactory;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured");
        }
    }

    public async Task<AIServiceResult<SkillExtractionResult>> ExtractSkillsFromResumeAsync(
        string resumeContent,
        string? languageCode = null)
    {
        _logger.LogInformation(
            "[LANG_AUDIT] Service={Service} Method={Method} LanguageCode={LanguageCode}",
            nameof(SkillExtractionService), "ExtractSkillsFromResumeAsync", languageCode);

        if (string.IsNullOrWhiteSpace(resumeContent))
        {
            _logger.LogWarning("Resume content is empty");
            return AIServiceResult<SkillExtractionResult>.Succeeded(new SkillExtractionResult { Skills = [] });
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Gemini API key is not configured");
            return AIServiceResult<SkillExtractionResult>.Failed(
                "GEMINI_NOT_CONFIGURED",
                "AI service temporarily unavailable.",
                new SkillExtractionResult { Skills = [] });
        }

        var sanitizedResumeContent = ResumeTextSanitizer.Sanitize(resumeContent, MaxResumeContentLength);
        _logger.LogInformation("[SkillExtraction] ORIGINAL_LENGTH={Length}", resumeContent.Length);
        _logger.LogInformation("[SkillExtraction] SANITIZED_LENGTH={Length}", sanitizedResumeContent.Length);

        var provider = _promptProviderFactory.Get(languageCode);
        var prompt = provider.BuildResumeSkillExtractionPrompt(sanitizedResumeContent);
        _logger.LogInformation("=== SKILL EXTRACTION DEBUG ===");
        _logger.LogInformation("Prompt: {Prompt}", prompt);

        var attemptResult = await SendGeminiRequestAsync(prompt, DefaultMaxOutputTokens);

        if (attemptResult.WasOutputTruncated)
        {
            _logger.LogWarning("[SkillExtraction] RETRY_ATTEMPT=1");
            _logger.LogWarning("[SkillExtraction] RETRY_MAX_OUTPUT_TOKENS=2000");
            attemptResult = await SendGeminiRequestAsync(prompt, RetryMaxOutputTokens);
        }

        var response = attemptResult.TransportResult;

        if (response is { Success: false })
        {
            _logger.LogInformation("Raw Gemini Response: <request failed>");
            _logger.LogInformation("Parsed DTO: <none>");
            _logger.LogInformation("Validation Result: Failed with {ErrorCode} - {Message}", response.Error?.ErrorCode, response.Error?.Message);
            return AIServiceResult<SkillExtractionResult>.Failed(
                response.Error?.ErrorCode ?? "GEMINI_ERROR",
                response.Error?.Message ?? "Unable to process request at this time.",
                new SkillExtractionResult { Skills = [] });
        }

        var parsedResult = attemptResult.ParsedResult;

        if (parsedResult is { Success: false })
        {
            _logger.LogInformation("Raw Gemini Response: {Response}", response?.Data);
            _logger.LogInformation("Parsed DTO: <none>");
            _logger.LogInformation("Validation Result: Failed with {ErrorCode} - {Message}", parsedResult.Error?.ErrorCode, parsedResult.Error?.Message);
            return parsedResult;
        }

        _logger.LogInformation("Raw Gemini Response: {Response}", response?.Data);
        _logger.LogInformation("Parsed DTO: {@ParsedDto}", parsedResult?.Data);
        _logger.LogInformation("Validation Result: {ValidationResult}", parsedResult?.Success);
        return parsedResult ?? AIServiceResult<SkillExtractionResult>.Failed(
            "INVALID_AI_RESPONSE",
            "AI service returned an invalid response.",
            new SkillExtractionResult { Skills = [] });
    }

    private async Task<SkillExtractionAttemptResult> SendGeminiRequestAsync(string prompt, int maxOutputTokens)
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
                maxOutputTokens,
                topP = 0.95,
                topK = 40,
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        skills = new
                        {
                            type = "ARRAY",
                            items = new
                            {
                                type = "STRING"
                            }
                        }
                    },
                    required = new[] { "skills" }
                }
            }
        };

        _logger.LogInformation(
            "[SkillExtraction] GENERATION_CONFIG maxOutputTokens={MaxOutputTokens} temperature={Temperature} topP={TopP} topK={TopK} responseMimeType={ResponseMimeType}",
            maxOutputTokens,
            0.1,
            0.95,
            40,
            "application/json");

        var jsonContent = JsonSerializer.Serialize(requestBody);
        _logger.LogInformation("[SkillExtraction] REQUEST_JSON={Json}", jsonContent);

        try
        {
            _logger.LogDebug("Sending request to Gemini API");

            for (var attempt = 1; attempt <= RetryCount; attempt++)
            {
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(
                    $"{GeminiApiUrl}?key={_apiKey}",
                    httpContent);

                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "[SkillExtraction] RAW_GEMINI_RESPONSE={Response}",
                    responseBody);

                _logger.LogInformation(
                    "[SkillExtraction] RESPONSE_LENGTH={Length}",
                    responseBody?.Length ?? 0);

                _logger.LogDebug("Gemini API response: {Response}", responseBody);

                if (response.IsSuccessStatusCode)
                {
                    var parsedResult = ParseGeminiResponse(responseBody);
                    return new SkillExtractionAttemptResult
                    {
                        TransportResult = AIServiceResult<string>.Succeeded(responseBody),
                        ParsedResult = parsedResult,
                        WasOutputTruncated = parsedResult.Error?.ErrorCode == "OUTPUT_TRUNCATED"
                    };
                }

                if (IsTransientStatusCode(response.StatusCode) && attempt < RetryCount)
                {
                    _logger.LogWarning("[SkillExtraction] RETRY_ATTEMPT={Attempt}", attempt);
                    _logger.LogWarning("[SkillExtraction] RETRY_REASON={StatusCode}", (int)response.StatusCode);
                    _logger.LogWarning("[SkillExtraction] GEMINI_UNAVAILABLE");

                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
                    continue;
                }

                _logger.LogError("Gemini API error: {StatusCode} - {Response}",
                    response.StatusCode, responseBody);

                if (IsTransientStatusCode(response.StatusCode))
                {
                    _logger.LogError("[SkillExtraction] GEMINI_UNAVAILABLE");
                    _logger.LogError("[SkillExtraction] RETRIES_EXHAUSTED");
                    return new SkillExtractionAttemptResult
                    {
                        TransportResult = CreateGeminiUnavailableFailure(response.StatusCode, responseBody)
                    };
                }

                return new SkillExtractionAttemptResult
                {
                    TransportResult = AIServiceResult<string>.Failed(
                        response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden
                            ? "GEMINI_AUTH_ERROR"
                            : "GEMINI_ERROR",
                        "Unable to process request at this time.",
                        responseBody)
                };
            }

            _logger.LogError("[SkillExtraction] RETRIES_EXHAUSTED");
            return new SkillExtractionAttemptResult
            {
                TransportResult = CreateGeminiUnavailableFailure(null, string.Empty)
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Gemini API request timed out");
            _logger.LogError("[SkillExtraction] GEMINI_UNAVAILABLE");
            _logger.LogError("[SkillExtraction] RETRIES_EXHAUSTED");
            return new SkillExtractionAttemptResult
            {
                TransportResult = CreateGeminiUnavailableFailure(HttpStatusCode.GatewayTimeout, ex.Message)
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API");
            _logger.LogError("[SkillExtraction] GEMINI_UNAVAILABLE");
            _logger.LogError("[SkillExtraction] RETRIES_EXHAUSTED");
            return new SkillExtractionAttemptResult
            {
                TransportResult = CreateGeminiUnavailableFailure(null, ex.Message)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Gemini skill extraction request");
            _logger.LogError("[SkillExtraction] GEMINI_UNAVAILABLE");
            _logger.LogError("[SkillExtraction] RETRIES_EXHAUSTED");
            return new SkillExtractionAttemptResult
            {
                TransportResult = CreateGeminiUnavailableFailure(null, ex.Message)
            };
        }
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
    }

    private static AIServiceResult<string> CreateGeminiUnavailableFailure(HttpStatusCode? statusCode, string rawResponse)
    {
        var diagnostics = new
        {
            Success = false,
            AiUsed = false,
            GeneratedBy = "FAILED",
            ErrorMessage = "Gemini service unavailable. Please try again later.",
            Diagnostics = new
            {
                HttpStatus = statusCode.HasValue ? (int)statusCode.Value : (int?)null,
                RawResponse = rawResponse,
                Model = GeminiModel
            }
        };

        return AIServiceResult<string>.Failed(
            "GEMINI_UNAVAILABLE",
            "AI service is temporarily busy. Please retry in a few minutes.",
            JsonSerializer.Serialize(diagnostics));
    }

    private AIServiceResult<SkillExtractionResult> ParseGeminiResponse(string responseBody)
    {
        try
        {
            _logger.LogInformation(
                "[SkillExtraction] RAW_GEMINI_RESPONSE={Response}",
                responseBody);

            _logger.LogInformation(
                "[SkillExtraction] RESPONSE_LENGTH={Length}",
                responseBody?.Length ?? 0);

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array &&
                candidates.GetArrayLength() > 0)
            {
                var candidate = candidates[0];
                var finishReason = candidate.TryGetProperty("finishReason", out var finishReasonElement)
                    ? finishReasonElement.GetString() ?? "OTHER"
                    : "OTHER";

                _logger.LogInformation(
                    "[SkillExtraction] FINISH_REASON={Reason}",
                    finishReason);

                if (candidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? string.Empty;

                    _logger.LogInformation(
                        "[SkillExtraction] GENERATED_TEXT={Text}",
                        text);

                    _logger.LogInformation(
                        "[SkillExtraction] GENERATED_TEXT_LENGTH={Length}",
                        text.Length);

                    if (string.Equals(finishReason, "MAX_TOKENS", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("[SkillExtraction] OUTPUT_TRUNCATED");
                        return AIServiceResult<SkillExtractionResult>.Failed(
                            "OUTPUT_TRUNCATED",
                            "Gemini output was truncated",
                            new SkillExtractionResult { Skills = [] });
                    }

                    return ExtractSkillsFromJsonText(text);
                }
            }

            _logger.LogWarning("Unexpected Gemini response format");
            return AIServiceResult<SkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an unexpected response.",
                new SkillExtractionResult { Skills = [] });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response");
            return AIServiceResult<SkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an invalid response.",
                new SkillExtractionResult { Skills = [] });
        }
    }

    private AIServiceResult<SkillExtractionResult> ExtractSkillsFromJsonText(string jsonText)
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

        if (result == null)
        {
            _logger.LogWarning("Could not extract skills from Gemini response text");
            return AIServiceResult<SkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an invalid response.",
                new SkillExtractionResult { Skills = [] });
        }

        return AIServiceResult<SkillExtractionResult>.Succeeded(result);
    }

    private SkillExtractionResult? TryDeserialize(string jsonText)
    {
        try
        {
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

                var normalizedSkills = NormalizeExtractedSkills(skills);
                _logger.LogInformation("[SkillExtraction] RAW_SKILLS={Skills}", string.Join(", ", skills));
                _logger.LogInformation("[SkillExtraction] NORMALIZED_SKILLS={Skills}", string.Join(", ", normalizedSkills));

                return new SkillExtractionResult
                {
                    Skills = normalizedSkills
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

    private SkillExtractionResult? TryExtractWithRegex(string text)
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

            var normalizedSkills = NormalizeExtractedSkills(skills);
            _logger.LogInformation("[SkillExtraction] RAW_SKILLS={Skills}", string.Join(", ", skills));
            _logger.LogInformation("[SkillExtraction] NORMALIZED_SKILLS={Skills}", string.Join(", ", normalizedSkills));

            return new SkillExtractionResult { Skills = normalizedSkills };
        }

        _logger.LogWarning("Could not extract skills using regex from response: {Text}", text);
        return null;
    }

    private static List<string> NormalizeExtractedSkills(IEnumerable<string> skills)
    {
        return skills
            .Select(skill => skill.Trim())
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Select(RemoveParentheticalDetails)
            .Select(skill => Regex.Replace(skill, @"[^\p{L}\p{Nd}\s]", " "))
            .Select(skill => Regex.Replace(skill, @"\s+", " ").Trim())
            .Select(skill => SkillNormalizationMap.TryGetValue(skill, out var mapped) ? mapped : skill)
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Where(skill => !ExcludedSkillPhrases.Any(phrase => skill.Contains(phrase, StringComparison.OrdinalIgnoreCase)))
            .Select(skill => NormalizeSkill(skill))
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string RemoveParentheticalDetails(string skill)
    {
        return Regex.Replace(skill, @"\([^)]*\)", string.Empty).Trim();
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
