namespace AIInterviewPlatform.Application.Common.Validators;

public interface IDashboardQueryValidator
{
    ValidationResult ValidateUserId(long userId);
}

public class ValidationResult
{
    public bool IsSuccess => Errors.Count == 0;
    public List<string> Errors { get; } = new();

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(string error)
    {
        var result = new ValidationResult();
        result.Errors.Add(error);
        return result;
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        var result = new ValidationResult();
        result.Errors.AddRange(errors);
        return result;
    }
}
