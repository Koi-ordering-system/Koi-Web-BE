using FluentValidation.Results;

namespace Koi_Web_BE.Exceptions;

public class RequestValidationException(List<ValidationFailure>? errors) : Exception("User input validation failed!")
{
    public List<ValidationFailure>? Errors { get; init; } = errors;
}
