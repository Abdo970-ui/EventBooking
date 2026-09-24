namespace EventBooking.Application.Common.Exceptions;

// Each maps to an HTTP status in GlobalExceptionHandler (API layer).
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} with id '{key}' was not found.") { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You are not allowed to perform this action.") : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized.") : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
