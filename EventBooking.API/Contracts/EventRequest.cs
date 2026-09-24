namespace EventBooking.API.Contracts;

/// <summary>Body for POST and PUT. The event id comes from the route, not the body.</summary>
public record EventRequest(string Title, string? Description, string Location, DateTime Date, int Capacity);
