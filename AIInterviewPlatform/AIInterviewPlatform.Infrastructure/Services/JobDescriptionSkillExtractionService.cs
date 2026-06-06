using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.JobDescriptionSkillExtraction;
using AIInterviewPlatform.Application.Interfaces.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIInterviewPlatform.Infrastructure.Services;

public class JobDescriptionSkillExtractionService : IJobDescriptionSkillExtractionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<JobDescriptionSkillExtractionService> _logger;
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public JobDescriptionSkillExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<JobDescriptionSkillExtractionService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<JobDescriptionSkillExtractionResult> ExtractRequiredSkillsAsync(
        string jobDescriptionContent)
    {
        if (string.IsNullOrWhiteSpace(jobDescriptionContent))
        {
            _logger.LogWarning("Job description content is empty");
            return new JobDescriptionSkillExtractionResult { RequiredSkills = [] };
        }

        var prompt = BuildPrompt(jobDescriptionContent);
        var response = await SendGeminiRequestAsync(prompt);
        
        return ParseGeminiResponse(response);
    }

    private string BuildPrompt(string jobDescriptionContent)
    {
        return $@"Analyze the following job description and extract ONLY the required skills, technologies, 
            programming languages, frameworks, and tools mentioned. Return ONLY a valid JSON object 
            with this exact format - no markdown, no explanation:
            {{""requiredSkills"": [""skill1"", ""skill2"", ""skill3""]}}
            
            Job Description:
            {jobDescriptionContent}
            
            Rules:
            - Return ONLY the JSON object
            - Use exact property name ""requiredSkills""
            - Each skill should be clean and standardized (e.g., ""C#"" not ""c#"", ""ASP.NET Core"" not ""asp.net"")
            - Remove duplicates if any
            - Include only technical skills, not soft skills";
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
                maxOutputTokens = 800
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(
                $"{GeminiApiUrl}?key={_apiKey}",
                httpContent);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API for job description skill extraction");
            throw new InvalidOperationException("Failed to extract required skills from job description", ex);
        }
    }

    private JobDescriptionSkillExtractionResult ParseGeminiResponse(string responseBody)
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

            _logger.LogWarning("Unexpected Gemini response format for job description");
            return new JobDescriptionSkillExtractionResult { RequiredSkills = [] };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response for job description");
            throw new InvalidOperationException("Failed to parse job description skill extraction response", ex);
        }
    }

    private JobDescriptionSkillExtractionResult ExtractSkillsFromJsonText(string jsonText)
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

        return result ?? new JobDescriptionSkillExtractionResult { RequiredSkills = [] };
    }

    private JobDescriptionSkillExtractionResult? TryDeserialize(string jsonText)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<JsonElement>(jsonText);
            
            if (parsed.TryGetProperty("requiredSkills", out var skillsElement) &&
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

                return new JobDescriptionSkillExtractionResult 
                { 
                    RequiredSkills = skills.Distinct().ToList() 
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

    private JobDescriptionSkillExtractionResult TryExtractWithRegex(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            text, 
            @"\{.*?""requiredSkills""\s*:\s*\[(.*?)\].*\}",
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

            return new JobDescriptionSkillExtractionResult { RequiredSkills = skills };
        }

        _logger.LogWarning("Could not extract skills using regex from response: {Text}", text);
        return new JobDescriptionSkillExtractionResult { RequiredSkills = [] };
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
            _ => normalized
        };

        return normalized;
    }
}
