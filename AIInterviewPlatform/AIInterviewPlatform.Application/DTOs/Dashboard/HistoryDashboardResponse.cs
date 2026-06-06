namespace AIInterviewPlatform.Application.DTOs.Dashboard;

public class AnalysisHistoryDto
{
    public long AnalysisId { get; set; }
    public long ResumeId { get; set; }
    public long JobDescriptionId { get; set; }
    public decimal ReadinessScore { get; set; }
    public int MatchedSkillsCount { get; set; }
    public int MissingSkillsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HistoryDashboardResponse
{
    public List<AnalysisHistoryDto> Analyses { get; set; } = [];
    public List<ReadinessTimelineDto> ReadinessTimeline { get; set; } = [];
    public decimal AverageScore { get; set; }
    public decimal HighestScore { get; set; }
    public decimal LowestScore { get; set; }
    public int TotalAnalyses { get; set; }
}

public class ReadinessTimelineDto
{
    public DateTime Date { get; set; }
    public decimal Score { get; set; }
}
