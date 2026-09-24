using Microsoft.AspNetCore.Identity;

namespace EventBooking.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = default!;
}
