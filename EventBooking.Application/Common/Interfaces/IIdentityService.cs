using EventBooking.Application.Features.Auth;

namespace EventBooking.Application.Common.Interfaces;

/// <summary>Abstraction over ASP.NET Identity so the Application layer never touches UserManager.</summary>
public interface IIdentityService
{
    Task<AuthResponse> RegisterAsync(string fullName, string email, string password, string role, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct = default);
}
