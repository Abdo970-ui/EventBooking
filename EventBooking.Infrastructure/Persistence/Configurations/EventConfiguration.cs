using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        // Ids are generated in code (Guid.NewGuid), not by the database.
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Location).HasMaxLength(300).IsRequired();
        builder.Property(e => e.OrganizerId).IsRequired();
        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasIndex(e => e.Date);

        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(e => e.OrganizerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Bookings)
               .WithOne()
               .HasForeignKey(b => b.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Bookings).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
