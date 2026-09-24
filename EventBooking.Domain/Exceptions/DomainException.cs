namespace EventBooking.Domain.Exceptions;

/// <summary>Thrown when a business rule is violated. Map to HTTP 400/409 in the global exception handler.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
