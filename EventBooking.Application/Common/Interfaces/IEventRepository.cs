using EventBooking.Application.Common.Models;
using EventBooking.Application.Features.Events;
using EventBooking.Domain.Entities;

namespace EventBooking.Application.Common.Interfaces;

public interface IEventRepository
{
    /// <summary>Loads the event with its bookings (needed for capacity/seat calculations).</summary>
    Task<Event?> GetByIdWithBookingsAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Event ev, CancellationToken ct = default);

    /// <summary>Active events only, ordered by date, with optional filters and pagination.</summary>
    Task<PagedResult<EventDto>> GetPagedAsync(
        string? location, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default);
}
