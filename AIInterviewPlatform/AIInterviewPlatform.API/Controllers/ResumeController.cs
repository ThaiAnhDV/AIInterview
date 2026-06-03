using AIInterviewPlatform.Application.DTOs.Resume;
using AIInterviewPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIInterviewPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;

        public ResumeController(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadResume(
    [FromForm] ResumeUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new
                {
                    message = "File is required"
                });
            }

            var userId = GetUserId();

            var result = await _resumeService.UploadResumeAsync(
                userId,
                request.File
            );

            return Ok(result);
        }

        [HttpGet("my-resumes")]
        public async Task<IActionResult> GetMyResumes()
        {
            var userId = GetUserId();
            var result = await _resumeService.GetMyResumesAsync(userId);
            return Ok(result);
        }

        [HttpPut("set-active/{resumeId}")]
        public async Task<IActionResult> SetActiveResume(long resumeId)
        {
            var userId = GetUserId();
            await _resumeService.SetActiveResumeAsync(userId, resumeId);
            return Ok(new { message = "Resume set active successfully" });
        }

        [HttpDelete("{resumeId}")]
        public async Task<IActionResult> DeleteResume(long resumeId)
        {
            var userId = GetUserId();
            await _resumeService.DeleteResumeAsync(userId, resumeId);
            return Ok(new { message = "Resume deleted successfully" });
        }

        [HttpGet("view/{resumeId}")]
        public async Task<IActionResult> ViewResume(long resumeId)
        {
            var userId = GetUserId();

            var resumes = await _resumeService.GetMyResumesAsync(userId);
            var resume = resumes.FirstOrDefault(x => x.Id == resumeId);

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found." });
            }

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                resume.FileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new
                {
                    message = "Physical file not found.",
                    path = physicalPath
                });
            }

            var contentType = GetResumeContentType(resume.FileName);
            return PhysicalFile(physicalPath, contentType);
        }

        [HttpGet("download/{resumeId}")]
        public async Task<IActionResult> DownloadResume(long resumeId)
        {
            var userId = GetUserId();

            var resumes = await _resumeService.GetMyResumesAsync(userId);
            var resume = resumes.FirstOrDefault(x => x.Id == resumeId);

            if (resume == null)
            {
                return NotFound(new { message = "Resume not found." });
            }

            var physicalPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                resume.FileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new
                {
                    message = "Physical file not found.",
                    path = physicalPath
                });
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

            return File(bytes, "application/octet-stream", resume.FileName);
        }

        private static string GetResumeContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream"
            };
        }

        private long GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                throw new Exception("UserId claim not found in token.");
            }

            return long.Parse(claim.Value);
        }
    }
}