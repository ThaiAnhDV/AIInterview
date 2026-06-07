namespace AIInterviewPlatform.Application.DTOs.Interview
{
    public class StartInterviewRequest
    {
        public long TargetJobId { get; set; }
        public string? LanguageCode { get; set; }
    }
}