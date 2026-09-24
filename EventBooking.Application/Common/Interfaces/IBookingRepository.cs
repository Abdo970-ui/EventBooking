using EventBooking.Application.Features.Bookings;
using EventBooking.Domain.Entities;

namespace EventBooking.Application.Common.Interfaces;

public interface IBookingRepository
{
    /// <summary>Tracked entity, used for cancelling.</summary>
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<BookingDto?> GetDtoByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<BookingDto>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
}
