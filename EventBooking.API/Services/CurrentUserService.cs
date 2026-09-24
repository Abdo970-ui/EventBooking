using System.Security.Claims;
using EventBooking.Application.Common.Interfaces;

namespace EventBooking.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public string? UserId => _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsInRole(string role) => _http.HttpContext?.User.IsInRole(role) ?? false;
}
