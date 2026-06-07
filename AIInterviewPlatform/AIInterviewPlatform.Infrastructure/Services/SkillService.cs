using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AIInterviewPlatform.Application.DTOs.Skill;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class SkillService : ISkillService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJobDescriptionSkillExtractionService _jobDescriptionSkillExtractionService;
        private readonly ILogger<SkillService> _logger;

        public SkillService(
            ApplicationDbContext context,
            IJobDescriptionSkillExtractionService jobDescriptionSkillExtractionService,
            ILogger<SkillService> logger)
        {
            _context = context;
            _jobDescriptionSkillExtractionService = jobDescriptionSkillExtractionService;
            _logger = logger;
        }

        public async Task<List<SkillResponse>> GetAllSkillsAsync()
        {
            return await _context.Skills
                .Select(x => new SkillResponse
                {
                    Id = x.Id,
                    SkillName = x.SkillName,
                    SkillType = x.SkillType
                })
                .ToListAsync();
        }

        public async Task<SkillResponse> CreateSkillAsync(
            CreateSkillRequest request)
        {
            var skill = new Skill
            {
                SkillName = request.SkillName,
                SkillType = request.SkillType
            };

            _context.Skills.Add(skill);

            await _context.SaveChangesAsync();

            return new SkillResponse
            {
                Id = skill.Id,
                SkillName = skill.SkillName,
                SkillType = skill.SkillType
            };
        }

        public async Task<List<RequiredSkillResponse>>
            GetRequiredSkillsByJobDescriptionAsync(
                long jobDescriptionId)
        {
            return await _context.RequiredSkills
                .Where(x => x.JobDescriptionId == jobDescriptionId)
                .Include(x => x.Skill)
                .Select(x => new RequiredSkillResponse
                {
                    Id = x.Id,
                    SkillId = x.SkillId,
                    SkillName = x.Skill.SkillName,
                    SkillType = x.Skill.SkillType,
                    ImportanceLevel = x.ImportanceLevel.ToString()
                })
                .ToListAsync();
        }

        public async Task<JobDescriptionSkillExtractionResponse> ExtractSkillsFromJobDescriptionAsync(
            long jobDescriptionId,
            string? languageCode = null)
        {
            _logger.LogInformation("[SkillExtraction] START");
            _logger.LogInformation("[SkillExtraction] JobDescriptionId={JobDescriptionId}", jobDescriptionId);

            var response = new JobDescriptionSkillExtractionResponse
            {
                Success = false,
                AiUsed = true,
                JobDescriptionId = jobDescriptionId,
                Model = _jobDescriptionSkillExtractionService.ModelName,
                Endpoint = _jobDescriptionSkillExtractionService.Endpoint,
                Diagnostics = new SkillExtractionDiagnosticsResponse
                {
                    Success = false,
                    AiUsed = true,
                    Model = _jobDescriptionSkillExtractionService.ModelName,
                    Endpoint = _jobDescriptionSkillExtractionService.Endpoint
                }
            };

            var jd = await _context.JobDescriptions
                .FirstOrDefaultAsync(x => x.Id == jobDescriptionId);

            if (jd == null)
            {
                response.AiUsed = false;
                response.ErrorMessage = "Job Description not found";
                response.Diagnostics.AiUsed = false;
                response.Diagnostics.ErrorMessage = response.ErrorMessage;
                _logger.LogWarning("[SkillExtraction] JobDescription not found for id {JobDescriptionId}", jobDescriptionId);
                _logger.LogInformation("[SkillExtraction] RESPONSE_SENT");
                return response;
            }

            _logger.LogInformation("[SkillExtraction] GEMINI_REQUEST_START");
            var prompt = BuildDiagnosticPrompt(jd.Content);
            _logger.LogInformation("[SkillExtraction] PROMPT:\n{Prompt}", prompt);

            var extractionResult = await _jobDescriptionSkillExtractionService.ExtractRequiredSkillsAsync(jd.Content);

            response.Diagnostics.RawResponse = extractionResult.Data == null
                ? string.Empty
                : System.Text.Json.JsonSerializer.Serialize(extractionResult.Data);

            if (!extractionResult.Success)
            {
                response.Success = false;
                response.ErrorMessage = extractionResult.Error?.Message ?? "Unable to process request at this time.";
                response.Diagnostics.Success = false;
                response.Diagnostics.ErrorMessage = response.ErrorMessage;
                response.Diagnostics.RawResponse = extractionResult.Data == null
                    ? string.Empty
                    : System.Text.Json.JsonSerializer.Serialize(extractionResult.Data);
                response.Diagnostics.HttpStatus = MapErrorCodeToStatusCode(extractionResult.Error?.ErrorCode);

                _logger.LogInformation("[SkillExtraction] GEMINI_RESPONSE:\n{RawResponse}", response.Diagnostics.RawResponse);
                _logger.LogInformation("[SkillExtraction] PARSED_SKILL_COUNT={Count}", 0);
                _logger.LogInformation("[SkillExtraction] RESPONSE_SENT");
                return response;
            }

            var parsedSkills = extractionResult.Data?.RequiredSkills
                ?.Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];

            response.ParsedSkillCount = parsedSkills.Count;

            _logger.LogInformation("[SkillExtraction] GEMINI_RESPONSE:\n{RawResponse}", response.Diagnostics.RawResponse);
            _logger.LogInformation("[SkillExtraction] PARSED_SKILL_COUNT={Count}", parsedSkills.Count);
            _logger.LogInformation("[SkillExtraction] SAVE_TO_DATABASE_START");

            foreach (var skillName in parsedSkills)
            {
                var normalizedSkillName = skillName.Trim();

                var existingSkill = await _context.Skills
                    .FirstOrDefaultAsync(x => x.SkillName.ToLower() == normalizedSkillName.ToLower());

                if (existingSkill == null)
                {
                    existingSkill = new Skill
                    {
                        SkillName = normalizedSkillName
                    };

                    _context.Skills.Add(existingSkill);
                    await _context.SaveChangesAsync();
                }

                var requiredSkillExists = await _context.RequiredSkills.AnyAsync(
                    x => x.JobDescriptionId == jobDescriptionId && x.SkillId == existingSkill.Id);

                if (!requiredSkillExists)
                {
                    _context.RequiredSkills.Add(
                        new RequiredSkill
                        {
                            JobDescriptionId = jobDescriptionId,
                            SkillId = existingSkill.Id
                        });
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[SkillExtraction] SAVE_TO_DATABASE_COMPLETE");

            response.Skills = await GetRequiredSkillsByJobDescriptionAsync(jobDescriptionId);
            response.DbSkillCount = response.Skills.Count;
            response.Success = true;
            response.ErrorMessage = string.Empty;
            response.Diagnostics.Success = true;
            response.Diagnostics.ErrorMessage = string.Empty;

            _logger.LogInformation("[SkillExtraction] DB_SKILL_COUNT={Count}", response.DbSkillCount);
            _logger.LogInformation("[SkillExtraction] RESPONSE_SENT");

            return response;
        }

        private static int? MapErrorCodeToStatusCode(string? errorCode)
        {
            return errorCode switch
            {
                "GEMINI_NOT_CONFIGURED" => 503,
                "TIMEOUT" => 504,
                "GEMINI_AUTH_ERROR" => 502,
                "INVALID_AI_RESPONSE" => 502,
                "GEMINI_ERROR" => 502,
                _ => null
            };
        }

        private static string BuildDiagnosticPrompt(string jobDescriptionContent)
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
    }
}
