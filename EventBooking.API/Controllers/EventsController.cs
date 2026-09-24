using EventBooking.API.Contracts;
using EventBooking.Application.Common.Models;
using EventBooking.Application.Features.Events;
using EventBooking.Application.Features.Events.Commands;
using EventBooking.Application.Features.Events.Queries;
using EventBooking.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventBooking.API.Controllers;

/// <summary>
/// Browse events (public) and manage them (Organizer only).
/// </summary>
[ApiController]
[Route("api/events")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly ISender _sender;

    public EventsController(ISender sender) => _sender = sender;

    /// <summary>Lists active events, ordered by date, with optional filters and paging.</summary>
    /// <remarks>
    /// Public endpoint. Query parameters (all optional):
    ///
    /// - <c>location</c>: events whose location contains this text.
    /// - <c>from</c> / <c>to</c>: date range, for example <c>2026-10-01</c>.
    /// - <c>page</c>: page number, starting at 1 (default 1).
    /// - <c>pageSize</c>: items per page, 1 to 50 (default 10).
    ///
    /// Cancelled events are not returned. <c>availableSeats</c> is capacity minus confirmed bookings.
    /// </remarks>
    /// <param name="query">Filters and paging options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">A page of events with the total count.</response>
    /// <response code="400">Invalid paging or date range.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<EventDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EventDto>>> GetAll([FromQuery] GetEventsQuery query, CancellationToken ct) =>
        Ok(await _sender.Send(query, ct));

    /// <summary>Gets one event by id.</summary>
    /// <remarks>Public endpoint. Cancelled events are returned too, with status <c>Cancelled</c>.</remarks>
    /// <param name="id">The event id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The event, including available seats.</response>
    /// <response code="404">No event with this id.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<EventDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await _sender.Send(new GetEventByIdQuery(id), ct));

    /// <summary>Creates an event owned by the current Organizer.</summary>
    /// <remarks>
    /// Requires the <c>Organizer</c> role. Sample request:
    ///
    ///     POST /api/events
    ///     {
    ///        "title": "Tech Meetup",
    ///        "description": "Monthly developers meetup",
    ///        "location": "Cairo",
    ///        "date": "2026-12-01T18:00:00Z",
    ///        "capacity": 50
    ///     }
    ///
    /// The date must be in the future and the capacity must be between 1 and 100000.
    /// </remarks>
    /// <param name="body">The event details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The event was created.</response>
    /// <response code="400">Validation failed (past date, invalid capacity, missing title or location).</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">The user is not an Organizer.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Organizer)]
    [ProducesResponseType<EventDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventDto>> Create(EventRequest body, CancellationToken ct)
    {
        var created = await _sender.Send(
            new CreateEventCommand(body.Title, body.Description, body.Location, body.Date, body.Capacity), ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an event. Only the Organizer who created it can do this.</summary>
    /// <remarks>
    /// Same body as creating an event. The capacity cannot be lower than the number of confirmed bookings.
    /// </remarks>
    /// <param name="id">The event id.</param>
    /// <param name="body">The new event details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated event.</response>
    /// <response code="400">Validation failed or a business rule was broken (for example capacity below bookings).</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">The user is not the organizer of this event.</response>
    /// <response code="404">No event with this id.</response>
    /// <response code="409">The event was changed by another request at the same time. Try again.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Organizer)]
    [ProducesResponseType<EventDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventDto>> Update(Guid id, EventRequest body, CancellationToken ct) =>
        Ok(await _sender.Send(
            new UpdateEventCommand(id, body.Title, body.Description, body.Location, body.Date, body.Capacity), ct));

    /// <summary>Cancels an event (soft delete). Only its Organizer can do this.</summary>
    /// <remarks>
    /// The event stays in the database with status <c>Cancelled</c>, disappears from the public list,
    /// and can no longer be booked. Existing bookings are kept.
    /// </remarks>
    /// <param name="id">The event id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The event was cancelled.</response>
    /// <response code="401">Missing or invalid token.</response>
    /// <response code="403">The user is not the organizer of this event.</response>
    /// <response code="404">No event with this id.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Organizer)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _sender.Send(new CancelEventCommand(id), ct);
        return NoContent();
    }
}