namespace AIInterviewPlatform.Application.DTOs.Resume
{
    public class ResumeResponse
    {
        public long Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}