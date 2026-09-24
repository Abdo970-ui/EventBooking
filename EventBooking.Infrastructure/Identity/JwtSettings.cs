namespace EventBooking.Infrastructure.Identity;

public class JwtSettings
{
    public string Key { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpiryMinutes { get; init; } = 60;
}
