using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using EventBooking.Application.Features.Auth;
using EventBooking.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace EventBooking.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenGenerator _jwt;

    public IdentityService(UserManager<ApplicationUser> userManager, JwtTokenGenerator jwt)
    {
        _userManager = userManager;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(string fullName, string email, string password, string role, CancellationToken ct = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
            throw new ConflictException("This email is already registered.");

        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            throw new BadRequestException(string.Join(" ", created.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, role);

        var (token, expires) = _jwt.Generate(user, role);
        return new AuthResponse(user.Id, user.FullName, email, role, token, expires);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // Same message for both cases so we don't reveal which emails exist.
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedException("Invalid email or password.");

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.Attendee;

        var (token, expires) = _jwt.Generate(user, role);
        return new AuthResponse(user.Id, user.FullName, user.Email!, role, token, expires);
    }
}
