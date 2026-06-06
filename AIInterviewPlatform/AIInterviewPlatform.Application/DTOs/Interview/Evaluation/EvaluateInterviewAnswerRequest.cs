using System.ComponentModel.DataAnnotations;

namespace AIInterviewPlatform.Application.DTOs.Interview.Evaluation;

public class EvaluateInterviewAnswerRequest
{
    [Required]
    public long AnswerId { get; set; }
}
