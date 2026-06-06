using Microsoft.EntityFrameworkCore;
using AIInterviewPlatform.Application.DTOs.Skill;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Infrastructure.Data;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class SkillService : ISkillService
    {
        private readonly ApplicationDbContext _context;

        public SkillService(ApplicationDbContext context)
        {
            _context = context;
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

        public async Task ExtractSkillsFromJobDescriptionAsync(
            long jobDescriptionId)
        {
            var jd = await _context.JobDescriptions
                .FirstOrDefaultAsync(x => x.Id == jobDescriptionId);

            if (jd == null)
                throw new Exception("Job Description not found");

            string content = jd.Content.ToLower();

            List<string> skillKeywords =
            [
                "c#",
                "asp.net",
                "sql",
                "entity framework",
                "rest api",
                "jwt",
                "git",
                "javascript",
                "react",
                "docker"
            ];

            foreach (var keyword in skillKeywords)
            {
                if (!content.Contains(keyword))
                    continue;

                var skill =
                    await _context.Skills
                    .FirstOrDefaultAsync(
                        x => x.SkillName.ToLower() == keyword);

                if (skill == null)
                {
                    skill = new Skill
                    {
                        SkillName = keyword
                    };

                    _context.Skills.Add(skill);

                    await _context.SaveChangesAsync();
                }

                bool exists =
                    await _context.RequiredSkills.AnyAsync(
                        x =>
                            x.JobDescriptionId == jobDescriptionId &&
                            x.SkillId == skill.Id);

                if (!exists)
                {
                    _context.RequiredSkills.Add(
                        new RequiredSkill
                        {
                            JobDescriptionId = jobDescriptionId,
                            SkillId = skill.Id
                        });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}