using EventBooking.Application.Common.Interfaces;
using EventBooking.Application.Common.Models;
using EventBooking.Application.Features.Events;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) => _db = db;

    public Task<Event?> GetByIdWithBookingsAsync(Guid id, CancellationToken ct = default) =>
        _db.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Event ev, CancellationToken ct = default) =>
        await _db.Events.AddAsync(ev, ct);

    public async Task<PagedResult<EventDto>> GetPagedAsync(
        string? location, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Events.AsNoTracking().Where(e => e.Status == EventStatus.Active);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(e => e.Location.Contains(location));

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(EventMappings.Projection) // computed in SQL, no need to load bookings
            .ToListAsync(ct);

        return new PagedResult<EventDto>(items, page, pageSize, total);
    }
}
