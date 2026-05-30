namespace SmartPark.SharedKernel;

public sealed class ValidationException : DomainException
{
    public ValidationException(IEnumerable<ValidationError> errors, string message = "请求校验失败。")
        : base(message, "validation_failed", 422, GroupErrors(errors))
    {
        Errors = errors.ToArray();
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }

    private static IReadOnlyDictionary<string, string[]> GroupErrors(IEnumerable<ValidationError> errors)
    {
        return errors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.Field) ? "request" : error.Field, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.Message)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }
}
