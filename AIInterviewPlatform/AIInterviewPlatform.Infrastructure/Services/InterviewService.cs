using AIInterviewPlatform.Application.DTOs.Interview;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using AIInterviewPlatform.Domain.Enum;
using AIInterviewPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly ApplicationDbContext _context;

        public InterviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InterviewSessionResponse> StartInterviewAsync(
            long userId,
            StartInterviewRequest request)
        {
            var targetJob = await _context.TargetJobs
                .FirstOrDefaultAsync(x =>
                    x.Id == request.TargetJobId &&
                    x.UserId == userId);

            if (targetJob == null)
            {
                throw new Exception("Target Job not found.");
            }

            var jobDescription = await _context.JobDescriptions
                .FirstOrDefaultAsync(x =>
                    x.TargetJobId == targetJob.Id);

            if (jobDescription == null)
            {
                throw new Exception("Job Description not found.");
            }

            var requiredSkills = await _context.RequiredSkills
                .Include(x => x.Skill)
                .Where(x => x.JobDescriptionId == jobDescription.Id)
                .ToListAsync();

            var session = new InterviewSession
            {
                UserId = userId,
                TargetJobId = targetJob.Id,
                SessionStatus = SessionStatus.IN_PROGRESS,
                StartedAt = DateTime.Now
            };

            _context.InterviewSessions.Add(session);

            await _context.SaveChangesAsync();

            foreach (var skill in requiredSkills)
            {
                _context.InterviewQuestions.Add(
                    new InterviewQuestion
                    {
                        InterviewSessionId = session.Id,
                        CategoryId = 1,
                        QuestionContent =
                            GenerateQuestion(skill.Skill.SkillName),
                        GeneratedBy = QuestionGeneratedBy.AI,
                        CreatedAt = DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(userId, session.Id)
                ?? throw new Exception("Session creation failed.");
        }

        public async Task<List<InterviewSessionResponse>>
            GetMySessionsAsync(long userId)
        {
            var sessions = await _context.InterviewSessions
                .Include(x => x.TargetJob)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.StartedAt)
                .ToListAsync();

            return sessions.Select(x => new InterviewSessionResponse
            {
                Id = x.Id,
                TargetJobId = x.TargetJobId,
                TargetJobTitle = x.TargetJob.JobTitle,
                SessionStatus = x.SessionStatus.ToString(),
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt
            }).ToList();
        }

        public async Task<InterviewSessionResponse?> GetByIdAsync(
            long userId,
            long sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(x => x.TargetJob)
                .FirstOrDefaultAsync(x =>
                    x.Id == sessionId &&
                    x.UserId == userId);

            if (session == null)
            {
                return null;
            }

            var questions = await _context.InterviewQuestions
                .Include(x => x.Category)
                .Where(x => x.InterviewSessionId == session.Id)
                .ToListAsync();

            return new InterviewSessionResponse
            {
                Id = session.Id,
                TargetJobId = session.TargetJobId,
                TargetJobTitle = session.TargetJob.JobTitle,
                SessionStatus = session.SessionStatus.ToString(),
                StartedAt = session.StartedAt,
                CompletedAt = session.CompletedAt,

                Questions = questions.Select(q =>
                    new InterviewQuestionResponse
                    {
                        Id = q.Id,
                        QuestionContent = q.QuestionContent,
                        CategoryName =
                            q.Category?.CategoryName ?? "General",
                        GeneratedBy =
                            q.GeneratedBy.ToString(),
                        CreatedAt = q.CreatedAt
                    }).ToList()
            };
        }

        public async Task<bool> CompleteSessionAsync(
            long userId,
            long sessionId)
        {
            var session = await _context.InterviewSessions
                .FirstOrDefaultAsync(x =>
                    x.Id == sessionId &&
                    x.UserId == userId);

            if (session == null)
            {
                return false;
            }

            session.SessionStatus = SessionStatus.COMPLETED;
            session.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateQuestion(string skill)
        {
            skill = skill.ToLower();

            if (skill.Contains("c#"))
                return "Explain the difference between class and interface in C#.";

            if (skill.Contains("asp.net"))
                return "Explain Dependency Injection in ASP.NET Core.";

            if (skill.Contains("sql"))
                return "How do you optimize SQL queries?";

            if (skill.Contains("entity framework"))
                return "What is the difference between EnsureCreated and Migration?";

            if (skill.Contains("jwt"))
                return "How does JWT Authentication work?";

            if (skill.Contains("git"))
                return "Explain Git Flow in a team project.";

            if (skill.Contains("docker"))
                return "What are the advantages of Docker?";

            if (skill.Contains("redis"))
                return "What is Redis and when would you use it?";

            return $"Explain your experience with {skill}.";
        }
    }
}