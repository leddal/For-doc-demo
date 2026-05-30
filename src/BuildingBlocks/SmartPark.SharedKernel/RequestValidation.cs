namespace SmartPark.SharedKernel;

public sealed record ValidationError(string Field, string Message);

public interface IRequestValidator<in TRequest>
{
    IReadOnlyCollection<ValidationError> Validate(TRequest request);
}

public static class RequestValidationExtensions
{
    public static void ValidateAndThrow<TRequest>(this IEnumerable<IRequestValidator<TRequest>> validators, TRequest request, string message = "请求校验失败。")
    {
        var errors = validators
            .SelectMany(validator => validator.Validate(request))
            .Where(error => !string.IsNullOrWhiteSpace(error.Message))
            .ToArray();

        if (errors.Length > 0)
        {
            throw new ValidationException(errors, message);
        }
    }
}
