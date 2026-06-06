namespace AIInterviewPlatform.Application.DTOs.Dashboard;

public class ReadinessScoreDto
{
    public decimal Score { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class ReadinessDashboardResponse
{
    public ReadinessScoreDto? LatestScore { get; set; }
    public ReadinessScoreDto? PreviousScore { get; set; }
    public decimal ImprovementPercentage { get; set; }
    public string Trend { get; set; } = "STABLE";
}
