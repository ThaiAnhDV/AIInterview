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

        public SkillGapAnalysisService(ApplicationDbContext context)
        {
            _context = context;
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
                        SkillName = x.Skill.SkillName,
                        GapLevel = x.GapLevel?.ToString() ?? "",
                        GapDescription = x.GapDescription
                    })
                    .ToList()
            };
        }

        public async Task<List<SkillGapAnalysisResponse>> GetMyAnalysesAsync(
            long userId)
        {
            var analyses = await _context.SkillGapAnalyses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var result = new List<SkillGapAnalysisResponse>();

            foreach (var analysis in analyses)
            {
                var score = await _context.ReadinessScores
                    .Where(x => x.SkillGapAnalysisId == analysis.Id)
                    .Select(x => x.Score)
                    .FirstOrDefaultAsync();

                var missingSkills = await _context.SkillGaps
                    .Include(x => x.Skill)
                    .Where(x => x.SkillGapAnalysisId == analysis.Id)
                    .ToListAsync();

                result.Add(new SkillGapAnalysisResponse
                {
                    Success = true,
                    Id = analysis.Id,
                    ResumeId = analysis.ResumeId,
                    JobDescriptionId = analysis.JobDescriptionId,
                    ReadinessScore = score,
                    CreatedAt = analysis.CreatedAt,
                    MissingSkills = missingSkills
                        .Select(x => new SkillGapItemResponse
                        {
                            SkillId = x.SkillId,
                            SkillName = x.Skill.SkillName,
                            GapLevel = x.GapLevel?.ToString() ?? "",
                            GapDescription = x.GapDescription
                        })
                        .ToList()
                });
            }

            return result;
        }

        public async Task<SkillGapAnalysisResponse?> GetByIdAsync(
            long userId,
            long analysisId)
        {
            var analysis = await _context.SkillGapAnalyses
                .FirstOrDefaultAsync(x =>
                    x.Id == analysisId &&
                    x.UserId == userId);

            if (analysis == null)
            {
                return null;
            }

            var score = await _context.ReadinessScores
                .Where(x => x.SkillGapAnalysisId == analysis.Id)
                .Select(x => x.Score)
                .FirstOrDefaultAsync();

            var missingSkills = await _context.SkillGaps
                .Include(x => x.Skill)
                .Where(x => x.SkillGapAnalysisId == analysis.Id)
                .ToListAsync();

            return new SkillGapAnalysisResponse
            {
                Success = true,
                Id = analysis.Id,
                ResumeId = analysis.ResumeId,
                JobDescriptionId = analysis.JobDescriptionId,
                ReadinessScore = score,
                CreatedAt = analysis.CreatedAt,
                MissingSkills = missingSkills
                    .Select(x => new SkillGapItemResponse
                    {
                        SkillId = x.SkillId,
                        SkillName = x.Skill.SkillName,
                        GapLevel = x.GapLevel?.ToString() ?? "",
                        GapDescription = x.GapDescription
                    })
                    .ToList()
            };
        }
    }
}
