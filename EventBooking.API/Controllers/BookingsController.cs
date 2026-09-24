using EventBooking.Application.Features.Bookings;
using EventBooking.Application.Features.Bookings.Commands;
using EventBooking.Application.Features.Bookings.Queries;
using EventBooking.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

/// <summary>
/// Booking seats (Attendee), viewing an event's bookings (Organizer) and cancelling bookings.
/// </summary>
[ApiController]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly ISender _sender;

    public BookingsController(ISender sender) => _sender = sender;

    /// <summary>Books one seat in an event for the current Attendee.</summary>
    /// <remarks>
    /// Requires the <c>Attendee</c> role. No request body is needed.
    ///
    /// The booking is rejected when the event is full, cancelled or already started,
    /// or when the user already has an active booking for it.
    /// </remarks>
    /// <param name="eventId">The event to book.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The booking was confirmed.</response>
    /// <response code="400">A business rule was broken (event full, cancelled, started, or already booked).</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">The user is not an Attendee.</response>
    /// <response code="404">No event with this id.</response>
    /// <response code="409">Another booking took the last seat at the same time, or a duplicate was detected. Try again.</response>
    [HttpPost("api/events/{eventId:guid}/bookings")]
    [Authorize(Roles = Roles.Attendee)]
    [ProducesResponseType<BookingDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> Book(Guid eventId, CancellationToken ct)
    {
        var booking = await _sender.Send(new BookEventCommand(eventId), ct);
        return StatusCode(StatusCodes.Status201Created, booking);
    }

    /// <summary>Lists every booking of an event, newest first (including cancelled ones).</summary>
    /// <remarks>Requires the <c>Organizer</c> role, and the organizer must own the event.</remarks>
    /// <param name="eventId">The event id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The bookings with attendee names.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">The user is not the organizer of this event.</response>
    /// <response code="404">No event with this id.</response>
    [HttpGet("api/events/{eventId:guid}/bookings")]
    [Authorize(Roles = Roles.Organizer)]
    [ProducesResponseType<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetForEvent(Guid eventId, CancellationToken ct) =>
        Ok(await _sender.Send(new GetEventBookingsQuery(eventId), ct));

    /// <summary>Lists the current user's own bookings, newest first.</summary>
    /// <remarks>Any authenticated user. Cancelled bookings are included with status <c>Cancelled</c>.</remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The user's bookings.</response>
    /// <response code="401">Missing or invalid token.</response>
    [HttpGet("api/bookings/my")]
    [Authorize]
    [ProducesResponseType<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetMine(CancellationToken ct) =>
        Ok(await _sender.Send(new GetMyBookingsQuery(), ct));

    /// <summary>Cancels one of the current user's bookings and frees the seat.</summary>
    /// <remarks>Only the user who made the booking can cancel it.</remarks>
    /// <param name="id">The booking id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The booking was cancelled.</response>
    /// <response code="400">The booking is already cancelled.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">The booking belongs to another user.</response>
    /// <response code="404">No booking with this id.</response>
    [HttpDelete("api/bookings/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CancelBookingCommand(id), ct);
        return NoContent();
    }
}