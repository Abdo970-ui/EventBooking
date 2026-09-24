using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using MediatR;

namespace EventBooking.Application.Features.Bookings.Commands;

public record CancelBookingCommand(Guid Id) : IRequest;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelBookingCommandHandler(IBookingRepository bookings, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _bookings = bookings;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelBookingCommand r, CancellationToken ct)
    {
        var booking = await _bookings.GetByIdAsync(r.Id, ct)
                      ?? throw new NotFoundException("Booking", r.Id);

        if (booking.UserId != _currentUser.UserId)
            throw new ForbiddenException("You can only cancel your own bookings.");

        booking.Cancel();
        await _uow.SaveChangesAsync(ct);
    }
}
