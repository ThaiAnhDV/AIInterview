using AIInterviewPlatform.Application.DTOs.TargetJob;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class TargetJobService : ITargetJobService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TargetJobService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TargetJobResponse> CreateTargetJobAsync(
            long userId,
            CreateTargetJobRequest request)
        {
            var targetJob = new TargetJob
            {
                UserId = userId,
                JobTitle = request.JobTitle,
                Industry = request.Industry,
                ExperienceLevel = request.ExperienceLevel,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.TargetJobs.AddAsync(targetJob);

            await _unitOfWork.SaveChangesAsync();

            return new TargetJobResponse
            {
                Id = targetJob.Id,
                JobTitle = targetJob.JobTitle,
                Industry = targetJob.Industry,
                ExperienceLevel = targetJob.ExperienceLevel,
                CreatedAt = targetJob.CreatedAt
            };
        }

        public async Task<List<TargetJobResponse>> GetMyTargetJobsAsync(
            long userId)
        {
            var targetJobs = await _unitOfWork.TargetJobs.FindAsync(
                x => x.UserId == userId
            );

            return targetJobs
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TargetJobResponse
                {
                    Id = x.Id,
                    JobTitle = x.JobTitle,
                    Industry = x.Industry,
                    ExperienceLevel = x.ExperienceLevel,
                    CreatedAt = x.CreatedAt
                })
                .ToList();
        }

        public async Task<TargetJobResponse> GetTargetJobByIdAsync(
            long userId,
            long targetJobId)
        {
            var targetJobs = await _unitOfWork.TargetJobs.FindAsync(
                x => x.Id == targetJobId
                  && x.UserId == userId
            );

            var targetJob = targetJobs.FirstOrDefault();

            if (targetJob == null)
            {
                throw new Exception("Target job not found.");
            }

            return new TargetJobResponse
            {
                Id = targetJob.Id,
                JobTitle = targetJob.JobTitle,
                Industry = targetJob.Industry,
                ExperienceLevel = targetJob.ExperienceLevel,
                CreatedAt = targetJob.CreatedAt
            };
        }

        public async Task<TargetJobResponse> UpdateTargetJobAsync(
            long userId,
            long targetJobId,
            UpdateTargetJobRequest request)
        {
            var targetJobs = await _unitOfWork.TargetJobs.FindAsync(
                x => x.Id == targetJobId
                  && x.UserId == userId
            );

            var targetJob = targetJobs.FirstOrDefault();

            if (targetJob == null)
            {
                throw new Exception("Target job not found.");
            }

            targetJob.JobTitle = request.JobTitle;
            targetJob.Industry = request.Industry;
            targetJob.ExperienceLevel = request.ExperienceLevel;
            targetJob.UpdatedAt = DateTime.Now;

            _unitOfWork.TargetJobs.Update(targetJob);

            await _unitOfWork.SaveChangesAsync();

            return new TargetJobResponse
            {
                Id = targetJob.Id,
                JobTitle = targetJob.JobTitle,
                Industry = targetJob.Industry,
                ExperienceLevel = targetJob.ExperienceLevel,
                CreatedAt = targetJob.CreatedAt
            };
        }

        public async Task<bool> DeleteTargetJobAsync(
            long userId,
            long targetJobId)
        {
            var targetJobs = await _unitOfWork.TargetJobs.FindAsync(
                x => x.Id == targetJobId
                  && x.UserId == userId
            );

            var targetJob = targetJobs.FirstOrDefault();

            if (targetJob == null)
            {
                throw new Exception("Target job not found.");
            }

            _unitOfWork.TargetJobs.Delete(targetJob);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<JobDescriptionResponse> AddJobDescriptionAsync(
            long userId,
            long targetJobId,
            CreateJobDescriptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                throw new Exception("Job description content is required.");
            }
            var targetJobs = await _unitOfWork.TargetJobs.FindAsync(
                x => x.Id == targetJobId
                  && x.UserId == userId
            );

            var targetJob = targetJobs.FirstOrDefault();

            if (targetJob == null)
            {
                throw new Exception("Target job not found.");
            }

            var jobDescription = new JobDescription
            {
                TargetJobId = targetJobId,
                Content = request.Content,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.JobDescriptions.AddAsync(jobDescription);

            await _unitOfWork.SaveChangesAsync();

            return new JobDescriptionResponse
            {
                Id = jobDescription.Id,
                TargetJobId = jobDescription.TargetJobId,
                Content = jobDescription.Content,
                CreatedAt = jobDescription.CreatedAt
            };
        }

        public async Task<JobDescriptionResponse?> GetJobDescriptionAsync(
            long userId,
            long targetJobId)
        {
            var targetJobs = await _unitOfWork.TargetJobs.FindAsync(
                x => x.Id == targetJobId
                  && x.UserId == userId
            );

            var targetJob = targetJobs.FirstOrDefault();

            if (targetJob == null)
            {
                throw new Exception("Target job not found.");
            }

            var jobDescriptions =
                await _unitOfWork.JobDescriptions.FindAsync(
                    x => x.TargetJobId == targetJobId
                );

            var jobDescription =
                jobDescriptions
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

            if (jobDescription == null)
            {
                return null;
            }

            return new JobDescriptionResponse
            {
                Id = jobDescription.Id,
                TargetJobId = jobDescription.TargetJobId,
                Content = jobDescription.Content,
                CreatedAt = jobDescription.CreatedAt
            };
        }
    }
}