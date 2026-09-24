using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        // 2601 / 2627 = unique index violation (e.g. the same user double-clicking "Book").
        // DbUpdateConcurrencyException is NOT caught here; GlobalExceptionHandler turns it into 409.
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("This record already exists (you may have already booked this event).");
        }
    }
}
