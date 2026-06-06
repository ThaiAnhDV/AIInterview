namespace AIInterviewPlatform.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }

    public ValidationException(string error)
        : base(error)
    {
        Errors = new List<string> { error };
    }
}
