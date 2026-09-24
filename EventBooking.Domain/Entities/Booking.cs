using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;

namespace EventBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid EventId { get; private set; }
    public string UserId { get; private set; } = default!;
    public BookingStatus Status { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private Booking() { } // EF Core

    internal static Booking Create(Guid eventId, string userId) => new()
    {
        EventId = eventId,
        UserId = userId,
        Status = BookingStatus.Confirmed
    };

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new DomainException("Booking is already cancelled.");

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }
}
