namespace EventBooking.Application.Features.Auth;

public record AuthResponse(
    string UserId,
    string FullName,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAt);
