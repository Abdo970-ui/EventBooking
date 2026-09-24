using EventBooking.Application.Common.Interfaces;
using EventBooking.Infrastructure.Identity;
using EventBooking.Infrastructure.Persistence;
using EventBooking.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 6;
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var jwt = new JwtSettings
        {
            Key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."),
            Issuer = config["Jwt:Issuer"] ?? "EventBooking",
            Audience = config["Jwt:Audience"] ?? "EventBooking",
            ExpiryMinutes = int.TryParse(config["Jwt:ExpiryMinutes"], out var m) ? m : 60
        };
        services.AddSingleton(jwt);
        services.AddSingleton<JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
