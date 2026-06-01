using AIInterviewPlatform.Application.Common.Validators;

namespace AIInterviewPlatform.Infrastructure.Validators;

public class DashboardQueryValidator : IDashboardQueryValidator
{
    private const long MinUserId = 1;
    private const long MaxUserId = long.MaxValue;

    public ValidationResult ValidateUserId(long userId)
    {
        if (userId < MinUserId)
        {
            return ValidationResult.Failure($"UserId must be greater than or equal to {MinUserId}");
        }

        if (userId > MaxUserId)
        {
            return ValidationResult.Failure($"UserId exceeds maximum allowed value");
        }

        return ValidationResult.Success();
    }
}
