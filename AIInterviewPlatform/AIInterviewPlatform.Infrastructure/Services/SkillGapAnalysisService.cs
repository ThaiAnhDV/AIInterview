using AIInterviewPlatform.Application.DTOs.Recommendation;
using AIInterviewPlatform.Application.DTOs.SkillGapAnalysis;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class SkillGapAnalysisService : ISkillGapAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRecommendationService _recommendationService;

        public SkillGapAnalysisService(
            ApplicationDbContext context,
            IRecommendationService recommendationService)
        {
            _context = context;
            _recommendationService = recommendationService;
        }

        public async Task<SkillGapAnalysisResponse> AnalyzeAsync(
            long userId,
            CreateSkillGapAnalysisRequest request)
        {
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ResumeId &&
                    x.UserId == userId);

            if (resume == null)
            {
                return new SkillGapAnalysisResponse
                {
                    Success = false,
                    ErrorCode = "RESUME_NOT_FOUND",
                    Message = "Resume not found.",
                    ResumeId = request.ResumeId,
                    JobDescriptionId = request.JobDescriptionId
                };
            }

            var requiredSkills = await _context.RequiredSkills
                .Include(x => x.Skill)
                .Where(x => x.JobDescriptionId == request.JobDescriptionId)
                .ToListAsync();

            if (!requiredSkills.Any())
            {
                return new SkillGapAnalysisResponse
                {
                    Success = false,
                    ErrorCode = "NO_REQUIRED_SKILLS",
                    Message = "No required skills found for this job description.",
                    ResumeId = request.ResumeId,
                    JobDescriptionId = request.JobDescriptionId
                };
            }

            var resumeContent =
                (resume.ParsedContent ?? "").ToLower();

            var matchedSkills = new List<string>();
            var matchedSkillEntities = new List<MatchedSkill>();
            var strengthReports = new List<StrengthWeaknessReport>();

            var analysis = new SkillGapAnalysis
            {
                UserId = userId,
                ResumeId = request.ResumeId,
                JobDescriptionId = request.JobDescriptionId,
                AnalysisStatus = AnalysisStatus.COMPLETED,
                CreatedAt = DateTime.Now
            };

            _context.SkillGapAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            foreach (var requiredSkill in requiredSkills)
            {
                var skillName = requiredSkill.Skill.SkillName;

                var isMatched =
                    resumeContent.Contains(skillName.ToLower());

                if (isMatched)
                {
                    matchedSkills.Add(skillName);

                    var matchedSkillEntity = new MatchedSkill
                    {
                        SkillGapAnalysisId = analysis.Id,
                        SkillId = requiredSkill.SkillId,
                        MatchScore = 1.0,
                        CreatedAt = DateTime.UtcNow
                    };
                    matchedSkillEntities.Add(matchedSkillEntity);

                    strengthReports.Add(new StrengthWeaknessReport
                    {
                        SkillGapAnalysisId = analysis.Id,
                        ReportType = ReportType.STRENGTH,
                        Content = $"Strong in {skillName}"
                    });
                }
                else
                {
                    var skillGap = new SkillGap
                    {
                        SkillGapAnalysisId = analysis.Id,
                        SkillId = requiredSkill.SkillId,
                        GapLevel = GapLevel.HIGH,
                        GapDescription = $"Missing skill: {skillName}"
                    };
                    _context.SkillGaps.Add(skillGap);
                }
            }

            _context.MatchedSkills.AddRange(matchedSkillEntities);
            await _context.SaveChangesAsync();

            decimal score =
                ((decimal)matchedSkills.Count / requiredSkills.Count) * 100;

            var readinessScore = new ReadinessScore
            {
                UserId = userId,
                SkillGapAnalysisId = analysis.Id,
                Score = Math.Round(score, 2),
                CalculatedAt = DateTime.Now
            };

            _context.ReadinessScores.Add(readinessScore);
            await _context.SaveChangesAsync();

            var missingSkills = await _context.SkillGaps
                .Include(x => x.Skill)
                .Where(x => x.SkillGapAnalysisId == analysis.Id)
                .ToListAsync();

            var weaknessReports = missingSkills.Select(gap => new StrengthWeaknessReport
            {
                SkillGapAnalysisId = analysis.Id,
                ReportType = ReportType.WEAKNESS,
                Content = $"Needs improvement in {gap.Skill?.SkillName ?? "Unknown skill"}"
            }).ToList();

            if (strengthReports.Any() || weaknessReports.Any())
            {
                _context.StrengthWeaknessReports.AddRange(strengthReports);
                _context.StrengthWeaknessReports.AddRange(weaknessReports);
                await _context.SaveChangesAsync();
            }

            var recommendations = await GenerateRecommendationsAsync(userId, analysis.Id, missingSkills);

            return new SkillGapAnalysisResponse
            {
                Success = true,
                Id = analysis.Id,
                ResumeId = analysis.ResumeId,
                JobDescriptionId = analysis.JobDescriptionId,
                ReadinessScore = readinessScore.Score,
                CreatedAt = analysis.CreatedAt,
                MatchedSkills = matchedSkills,
                MissingSkills = missingSkills
                    .Select(x => new SkillGapItemResponse
                    {
                        SkillId = x.SkillId,
                        SkillName = x.Skill?.SkillName ?? "",
                        GapLevel = x.GapLevel?.ToString() ?? "",
                        GapDescription = x.GapDescription
                    })
                    .ToList(),
                Strengths = strengthReports.Select(r => new StrengthWeaknessReportResponse
                {
                    Id = r.Id,
                    ReportType = MapReportType(r.ReportType),
                    Content = r.Content
                }).ToList(),
                Weaknesses = weaknessReports.Select(r => new StrengthWeaknessReportResponse
                {
                    Id = r.Id,
                    ReportType = MapReportType(r.ReportType),
                    Content = r.Content
                }).ToList(),
                Recommendations = recommendations
            };
        }

        private async Task<List<RecommendationResponse>> GenerateRecommendationsAsync(
            long userId,
            long skillGapAnalysisId,
            List<SkillGap> missingSkills)
        {
            if (!missingSkills.Any())
            {
                return new List<RecommendationResponse>();
            }

            var missingSkillInputs = missingSkills.Select(s => new MissingSkillInput
            {
                SkillId = s.SkillId,
                SkillName = s.Skill?.SkillName ?? string.Empty,
                GapDescription = s.GapDescription
            }).ToList();

            return await _recommendationService.GenerateAndSaveRecommendationsAsync(
                userId,
                skillGapAnalysisId,
                missingSkillInputs);
        }

        public async Task<List<SkillGapAnalysisResponse>> GetMyAnalysesAsync(
            long userId)
        {
            var analyses = await _context.SkillGapAnalyses
                .AsSplitQuery()
                .AsNoTracking()
                .Include(x => x.ReadinessScores)
                .Include(x => x.SkillGaps)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.MatchedSkills)
                    .ThenInclude(x => x.Skill)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return analyses.Select(analysis =>
            {
                var score = analysis.ReadinessScores
                    .FirstOrDefault()?.Score ?? 0;

                var missingSkills = analysis.SkillGaps
                    .Select(x => new SkillGapItemResponse
                    {
                        SkillId = x.SkillId,
                        SkillName = x.Skill?.SkillName ?? "",
                        GapLevel = x.GapLevel?.ToString() ?? "",
                        GapDescription = x.GapDescription
                    })
                    .ToList();

                var matchedSkills = analysis.MatchedSkills
                    .Select(x => x.Skill?.SkillName ?? "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                return new SkillGapAnalysisResponse
                {
                    Success = true,
                    Id = analysis.Id,
                    ResumeId = analysis.ResumeId,
                    JobDescriptionId = analysis.JobDescriptionId,
                    ReadinessScore = score,
                    CreatedAt = analysis.CreatedAt,
                    MatchedSkills = matchedSkills,
                    MissingSkills = missingSkills
                };
            }).ToList();
        }

        public async Task<SkillGapAnalysisResponse?> GetByIdAsync(
            long userId,
            long analysisId)
        {
            var analysis = await _context.SkillGapAnalyses
                .AsSplitQuery()
                .AsNoTracking()
                .Include(x => x.ReadinessScores)
                .Include(x => x.SkillGaps)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.MatchedSkills)
                    .ThenInclude(x => x.Skill)
                .Include(x => x.StrengthWeaknessReports)
                .Include(x => x.Recommendations)
                    .ThenInclude(r => r.Skill)
                .FirstOrDefaultAsync(x =>
                    x.Id == analysisId &&
                    x.UserId == userId);

            if (analysis == null)
            {
                return null;
            }

            var score = analysis.ReadinessScores
                .FirstOrDefault()?.Score ?? 0;

            var missingSkills = analysis.SkillGaps
                .Select(x => new SkillGapItemResponse
                {
                    SkillId = x.SkillId,
                    SkillName = x.Skill?.SkillName ?? "",
                    GapLevel = x.GapLevel?.ToString() ?? "",
                    GapDescription = x.GapDescription
                })
                .ToList();

            var matchedSkills = analysis.MatchedSkills
                .Select(x => x.Skill?.SkillName ?? "")
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            var strengths = analysis.StrengthWeaknessReports
                .Where(r => r.ReportType == ReportType.STRENGTH)
                .Select(r => new StrengthWeaknessReportResponse
                {
                    Id = r.Id,
                    ReportType = ReportTypeResponse.STRENGTH,
                    Content = r.Content
                })
                .ToList();

            var weaknesses = analysis.StrengthWeaknessReports
                .Where(r => r.ReportType == ReportType.WEAKNESS)
                .Select(r => new StrengthWeaknessReportResponse
                {
                    Id = r.Id,
                    ReportType = ReportTypeResponse.WEAKNESS,
                    Content = r.Content
                })
                .ToList();

            var recommendations = analysis.Recommendations
                .Select(r => new RecommendationResponse
                {
                    Id = r.Id,
                    SkillGapAnalysisId = r.SkillGapAnalysisId,
                    SkillId = r.SkillId,
                    SkillName = r.Skill?.SkillName ?? string.Empty,
                    RecommendationTitle = r.RecommendationTitle,
                    RecommendationContent = r.RecommendationContent,
                    RecommendationType = r.RecommendationType?.ToString() ?? string.Empty,
                    PriorityLevel = r.PriorityLevel.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            return new SkillGapAnalysisResponse
            {
                Success = true,
                Id = analysis.Id,
                ResumeId = analysis.ResumeId,
                JobDescriptionId = analysis.JobDescriptionId,
                ReadinessScore = score,
                CreatedAt = analysis.CreatedAt,
                MatchedSkills = matchedSkills,
                MissingSkills = missingSkills,
                Strengths = strengths,
                Weaknesses = weaknesses,
                Recommendations = recommendations
            };
        }

        private static ReportTypeResponse MapReportType(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.STRENGTH => ReportTypeResponse.STRENGTH,
                ReportType.WEAKNESS => ReportTypeResponse.WEAKNESS,
                _ => ReportTypeResponse.STRENGTH
            };
        }
    }
}
