using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        // IMPORTANT: the Guid is assigned in code. Without this, EF treats a new Booking added to a tracked
        // Event as an existing row and tries to UPDATE it instead of INSERT.
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.Status).HasConversion<int>();

        // DB-level safety net for "no duplicate active booking" (1 = Confirmed). SQL Server syntax.
        builder.HasIndex(b => new { b.EventId, b.UserId })
               .IsUnique()
               .HasFilter("[Status] = 1");

        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
