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
        private readonly IResumeParserService _resumeParserService;

        public ResumeService(
            IUnitOfWork unitOfWork,
            IResumeParserService resumeParserService)
        {
            _unitOfWork = unitOfWork;
            _resumeParserService = resumeParserService;
        }

        public async Task<ResumeResponse> UploadResumeAsync(long userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is required.");
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Only PDF/DOC/DOCX files are allowed.");
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "resumes");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string? parsedContent = null;

            try
            {
                parsedContent = await _resumeParserService.ExtractTextAsync(filePath, file.FileName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Resume parsing failed for user {userId}, file '{file.FileName}': {ex.Message}");
            }

            var resume = new Resume
            {
                UserId = userId,
                FileName = file.FileName,
                FileUrl = $"/uploads/resumes/{uniqueFileName}",
                UploadedAt = DateTime.UtcNow,
                IsActive = false,
                ParsedContent = parsedContent
            };

            await _unitOfWork.Resumes.AddAsync(resume);
            await _unitOfWork.SaveChangesAsync();

            return new ResumeResponse
            {
                Id = resume.Id,
                FileName = resume.FileName,
                FileUrl = resume.FileUrl,
                IsActive = resume.IsActive,
                UploadedAt = resume.UploadedAt
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
