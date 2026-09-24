using EventBooking.Application.Common.Interfaces;
using EventBooking.Domain.Constants;
using FluentValidation;
using MediatR;

namespace EventBooking.Application.Features.Auth.Commands;

public record RegisterCommand(string FullName, string Email, string Password, string Role) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6)
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.Role)
            .Must(r => r is Roles.Organizer or Roles.Attendee)
            .WithMessage($"Role must be '{Roles.Organizer}' or '{Roles.Attendee}'.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService _identity;

    public RegisterCommandHandler(IIdentityService identity) => _identity = identity;

    public Task<AuthResponse> Handle(RegisterCommand r, CancellationToken ct) =>
        _identity.RegisterAsync(r.FullName, r.Email, r.Password, r.Role, ct);
}
