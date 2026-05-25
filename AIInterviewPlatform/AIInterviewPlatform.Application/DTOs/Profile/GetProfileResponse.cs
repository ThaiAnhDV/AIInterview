namespace AIInterviewPlatform.Application.DTOs.Profile
{
    public class GetProfileResponse
    {
        public long UserId { get; set; }

        public string? FullName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? EducationLevel { get; set; }

        public string? CareerGoal { get; set; }
    }
}