using System.Text.Json.Serialization;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.Features.Events;

public record EventDto(
    Guid Id,
    string Title,
    string? Description,
    string Location,
    DateTime Date,
    int Capacity,
    int AvailableSeats,
    string OrganizerId,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] EventStatus Status);
