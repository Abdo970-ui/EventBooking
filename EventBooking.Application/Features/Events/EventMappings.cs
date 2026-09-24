using System.Linq.Expressions;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.Features.Events;

public static class EventMappings
{
    /// <summary>
    /// Single source of truth for Event -> EventDto.
    /// It is an Expression so EF Core can translate it to SQL (used by the paged query),
    /// and it is compiled once for mapping already-loaded entities.
    /// </summary>
    public static readonly Expression<Func<Event, EventDto>> Projection = e => new EventDto(
        e.Id,
        e.Title,
        e.Description,
        e.Location,
        e.Date,
        e.Capacity,
        e.Capacity - e.Bookings.Count(b => b.Status == BookingStatus.Confirmed),
        e.OrganizerId,
        e.Status);

    private static readonly Func<Event, EventDto> Compiled = Projection.Compile();

    public static EventDto ToDto(this Event e) => Compiled(e);
}
