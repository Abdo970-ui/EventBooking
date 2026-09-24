using EventBooking.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace EventBooking.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identity;

    public LoginCommandHandler(IIdentityService identity) => _identity = identity;

    public Task<AuthResponse> Handle(LoginCommand r, CancellationToken ct) =>
        _identity.LoginAsync(r.Email, r.Password, ct);
}
