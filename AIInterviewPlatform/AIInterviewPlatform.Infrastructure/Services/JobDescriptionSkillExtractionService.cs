using System.Text;
using System.Text.Json;

using AIInterviewPlatform.Application.DTOs.AI;
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
    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public JobDescriptionSkillExtractionService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<JobDescriptionSkillExtractionService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiSettings:ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<AIServiceResult<JobDescriptionSkillExtractionResult>> ExtractRequiredSkillsAsync(
        string jobDescriptionContent)
    {
        if (string.IsNullOrWhiteSpace(jobDescriptionContent))
        {
            _logger.LogWarning("Job description content is empty");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Succeeded(
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Gemini API key is not configured for job description extraction");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "GEMINI_NOT_CONFIGURED",
                "AI service temporarily unavailable.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        var prompt = BuildPrompt(jobDescriptionContent);
        _logger.LogInformation("=== JD EXTRACTION DEBUG ===");
        _logger.LogInformation("Prompt: {Prompt}", prompt);
        var response = await SendGeminiRequestAsync(prompt);

        if (!response.Success)
        {
            _logger.LogInformation("Raw Gemini Response: <request failed>");
            _logger.LogInformation("Parsed DTO: <none>");
            _logger.LogInformation("Validation Result: Failed with {ErrorCode} - {Message}", response.Error?.ErrorCode, response.Error?.Message);
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                response.Error?.ErrorCode ?? "GEMINI_ERROR",
                response.Error?.Message ?? "Unable to process request at this time.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        _logger.LogInformation("Raw Gemini Response: {Response}", response.Data);
        var parsedResult = ParseGeminiResponse(response.Data ?? string.Empty);
        _logger.LogInformation("Parsed DTO: {@ParsedDto}", parsedResult.Data);
        _logger.LogInformation("Validation Result: {ValidationResult}", parsedResult.Success);
        return parsedResult;
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

    private async Task<AIServiceResult<string>> SendGeminiRequestAsync(string prompt)
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

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API returned {StatusCode} for job description extraction: {Response}", response.StatusCode, responseBody);
                return AIServiceResult<string>.Failed(
                    "GEMINI_ERROR",
                    "Unable to process request at this time.");
            }

            return AIServiceResult<string>.Succeeded(responseBody);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Gemini API timeout for job description skill extraction");
            return AIServiceResult<string>.Failed(
                "TIMEOUT",
                "AI service temporarily unavailable.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API for job description skill extraction");
            return AIServiceResult<string>.Failed(
                "GEMINI_ERROR",
                "Unable to process request at this time.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during job description skill extraction");
            return AIServiceResult<string>.Failed(
                "GEMINI_ERROR",
                "Unable to process request at this time.");
        }
    }

    private AIServiceResult<JobDescriptionSkillExtractionResult> ParseGeminiResponse(string responseBody)
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
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an unexpected response.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response for job description");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an invalid response.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }
    }

    private AIServiceResult<JobDescriptionSkillExtractionResult> ExtractSkillsFromJsonText(string jsonText)
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
            _logger.LogWarning("Could not extract required skills from Gemini response text");
            return AIServiceResult<JobDescriptionSkillExtractionResult>.Failed(
                "INVALID_AI_RESPONSE",
                "AI service returned an invalid response.",
                new JobDescriptionSkillExtractionResult { RequiredSkills = [] });
        }

        return AIServiceResult<JobDescriptionSkillExtractionResult>.Succeeded(result);
    }

    private JobDescriptionSkillExtractionResult? TryDeserialize(string jsonText)
    {
        try
        {
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

    private JobDescriptionSkillExtractionResult? TryExtractWithRegex(string text)
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
        return null;
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
