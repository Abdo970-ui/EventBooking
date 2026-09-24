using System.Text.Json.Serialization;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.Features.Bookings;

public record BookingDto(
    Guid Id,
    Guid EventId,
    string EventTitle,
    DateTime EventDate,
    string UserId,
    string AttendeeName,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] BookingStatus Status,
    DateTime CreatedAt,
    DateTime? CancelledAt);
