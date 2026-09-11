namespace ECommerce.Application.Exceptions;

/// <summary>Thrown whenever a requested resource could not be found. Results in HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Thrown for validation errors. Results in HTTP 400 with a list of error messages.</summary>
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors) : base(errors.FirstOrDefault() ?? "Validation failed")
        => Errors = errors;
}

/// <summary>Thrown when an authenticated user lacks the required permission. Results in HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>A lightweight exception to represent a failed authentication/authorization flow with a message.</summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

/// <summary>Thrown for general business-rule violations. Results in HTTP 400.</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}