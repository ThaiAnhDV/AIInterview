using AIInterviewPlatform.Application.DTOs.Resume;
using AIInterviewPlatform.Application.Interfaces.Repositories;
using AIInterviewPlatform.Application.Interfaces.Services;
using AIInterviewPlatform.Domain.Enities;
using Microsoft.AspNetCore.Http;

namespace AIInterviewPlatform.Infrastructure.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ResumeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResumeResponse> UploadResumeAsync(long userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is required.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
            {
                throw new Exception("Only PDF, DOC, DOCX files are allowed.");
            }

            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }

            var uploadFolder = Path.Combine(webRoot, "uploads", "resumes");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(uploadFolder, storedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/resumes/{storedFileName}";

            var oldResumes = await _unitOfWork.Resumes.FindAsync(x => x.UserId == userId);

            foreach (var oldResume in oldResumes)
            {
                oldResume.IsActive = false;
                _unitOfWork.Resumes.Update(oldResume);
            }

            var resume = new Resume
            {
                UserId = userId,
                FileName = file.FileName,
                FileUrl = fileUrl,
                UploadedAt = DateTime.Now,
                IsActive = true
            };

            await _unitOfWork.Resumes.AddAsync(resume);
            await _unitOfWork.SaveChangesAsync();

            return new ResumeResponse
            {
                Id = resume.Id,
                FileName = resume.FileName,
                FileUrl = resume.FileUrl,
                UploadedAt = resume.UploadedAt,
                IsActive = resume.IsActive
            };
        }

        public async Task<List<ResumeResponse>> GetMyResumesAsync(long userId)
        {
            var resumes = await _unitOfWork.Resumes.FindAsync(x => x.UserId == userId);

            return resumes
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => new ResumeResponse
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    FileUrl = x.FileUrl,
                    IsActive = x.IsActive,
                    UploadedAt = x.UploadedAt
                })
                .ToList();
        }

        public async Task<bool> SetActiveResumeAsync(long userId, long resumeId)
        {
            var resumes = await _unitOfWork.Resumes.FindAsync(x => x.UserId == userId);

            var selectedResume = resumes.FirstOrDefault(x => x.Id == resumeId);

            if (selectedResume == null)
            {
                throw new Exception("Resume not found.");
            }

            foreach (var resume in resumes)
            {
                resume.IsActive = resume.Id == resumeId;
                _unitOfWork.Resumes.Update(resume);
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteResumeAsync(long userId, long resumeId)
        {
            var resumes = await _unitOfWork.Resumes.FindAsync(
                x => x.UserId == userId && x.Id == resumeId
            );

            var resume = resumes.FirstOrDefault();

            if (resume == null)
            {
                throw new Exception("Resume not found.");
            }

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                resume.FileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            _unitOfWork.Resumes.Delete(resume);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}