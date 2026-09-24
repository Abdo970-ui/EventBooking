using EventBooking.Application.Features.Auth;
using EventBooking.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

/// <summary>
/// Registration and login. Both endpoints return a JWT; send it on later requests
/// as <c>Authorization: Bearer {token}</c>.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    /// <summary>Registers a new user and returns a JWT.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/register
    ///     {
    ///        "fullName": "Ahmed Ali",
    ///        "email": "ahmed@test.com",
    ///        "password": "Pass123",
    ///        "role": "Organizer"
    ///     }
    ///
    /// Allowed roles: <c>Organizer</c> or <c>Attendee</c>.
    /// The password needs at least 6 characters and one digit.
    /// </remarks>
    /// <param name="command">Name, email, password and role of the new user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The user was created. The response contains the JWT and its expiry.</response>
    /// <response code="400">Validation failed (invalid email, weak password, unknown role).</response>
    /// <response code="409">The email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterCommand command, CancellationToken ct) =>
        Ok(await _sender.Send(command, ct));

    /// <summary>Logs in with email and password and returns a JWT.</summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///        "email": "ahmed@test.com",
    ///        "password": "Pass123"
    ///     }
    /// </remarks>
    /// <param name="command">Email and password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Login succeeded. The response contains the JWT and its expiry.</response>
    /// <response code="400">Validation failed (missing or malformed fields).</response>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginCommand command, CancellationToken ct) =>
        Ok(await _sender.Send(command, ct));
}