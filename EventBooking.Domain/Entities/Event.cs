using EventBooking.Domain.Common;
using EventBooking.Domain.Enums;
using EventBooking.Domain.Exceptions;

namespace EventBooking.Domain.Entities;

public class Event : BaseEntity
{
    private readonly List<Booking> _bookings = new();

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Location { get; private set; } = default!;
    public DateTime Date { get; private set; }
    public int Capacity { get; private set; }
    public string OrganizerId { get; private set; } = default!;
    public EventStatus Status { get; private set; }

    /// <summary>
    /// Touched on every change to the event (including a new booking). This forces an UPDATE on the Event row,
    /// which makes the RowVersion check run, so two people can't take the last seat at the same time.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Optimistic concurrency token.</summary>
    public byte[] RowVersion { get; private set; } = default!;

    public IReadOnlyCollection<Booking> Bookings => _bookings.AsReadOnly();

    /// <remarks>Requires Bookings to be loaded (Include) to be accurate.</remarks>
    public int ConfirmedCount => _bookings.Count(b => b.Status == BookingStatus.Confirmed);
    public int AvailableSeats => Capacity - ConfirmedCount;

    private Event() { } // EF Core

    public static Event Create(string title, string? description, string location,
                               DateTime date, int capacity, string organizerId)
    {
        Validate(date, capacity);

        return new Event
        {
            Title = title,
            Description = description,
            Location = location,
            Date = date,
            Capacity = capacity,
            OrganizerId = organizerId,
            Status = EventStatus.Active
        };
    }

    public void Update(string title, string? description, string location, DateTime date, int capacity)
    {
        Validate(date, capacity);

        if (capacity < ConfirmedCount)
            throw new DomainException("Capacity cannot be lower than the number of confirmed bookings.");

        Title = title;
        Description = description;
        Location = location;
        Date = date;
        Capacity = capacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public Booking Book(string userId)
    {
        if (Status == EventStatus.Cancelled)
            throw new DomainException("This event has been cancelled.");

        if (Date <= DateTime.UtcNow)
            throw new DomainException("Cannot book an event that has already started or passed.");

        if (_bookings.Any(b => b.UserId == userId && b.Status == BookingStatus.Confirmed))
            throw new DomainException("You have already booked this event.");

        if (ConfirmedCount >= Capacity)
            throw new DomainException("This event is fully booked.");

        var booking = Booking.Create(Id, userId);
        _bookings.Add(booking);
        UpdatedAt = DateTime.UtcNow; // triggers the RowVersion concurrency check on save
        return booking;
    }

    private static void Validate(DateTime date, int capacity)
    {
        if (capacity <= 0)
            throw new DomainException("Capacity must be greater than zero.");

        if (date <= DateTime.UtcNow)
            throw new DomainException("Event date must be in the future.");
    }
}
