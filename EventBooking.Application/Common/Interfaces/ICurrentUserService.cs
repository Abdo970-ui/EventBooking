namespace EventBooking.Application.Common.Interfaces;

/// <summary>Who is calling the API right now (read from the JWT). Implemented in the API layer.</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsInRole(string role);
}
