using System.Linq.Expressions;
using EventBooking.Application.Common.Interfaces;
using EventBooking.Application.Features.Bookings;
using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db) => _db = db;

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<BookingDto?> GetDtoByIdAsync(Guid id, CancellationToken ct = default) =>
        (await ProjectAsync(b => b.Id == id, ct)).FirstOrDefault();

    public async Task<IReadOnlyList<BookingDto>> GetByUserAsync(string userId, CancellationToken ct = default) =>
        await ProjectAsync(b => b.UserId == userId, ct);

    public async Task<IReadOnlyList<BookingDto>> GetByEventAsync(Guid eventId, CancellationToken ct = default) =>
        await ProjectAsync(b => b.EventId == eventId, ct);

    // Joins Booking + Event + User in one SQL query (filter is applied before the projection).
    private async Task<List<BookingDto>> ProjectAsync(Expression<Func<Booking, bool>> filter, CancellationToken ct)
    {
        var query = from b in _db.Bookings.AsNoTracking().Where(filter)
                    join e in _db.Events on b.EventId equals e.Id
                    join u in _db.Users on b.UserId equals u.Id
                    orderby b.CreatedAt descending
                    select new BookingDto(
                        b.Id, b.EventId, e.Title, e.Date,
                        b.UserId, u.FullName, b.Status, b.CreatedAt, b.CancelledAt);

        return await query.ToListAsync(ct);
    }
}
