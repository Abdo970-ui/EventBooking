using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using MediatR;

namespace EventBooking.Application.Features.Bookings.Queries;

public record GetMyBookingsQuery : IRequest<IReadOnlyList<BookingDto>>;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IBookingRepository _bookings;
    private readonly ICurrentUserService _currentUser;

    public GetMyBookingsQueryHandler(IBookingRepository bookings, ICurrentUserService currentUser)
    {
        _bookings = bookings;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<BookingDto>> Handle(GetMyBookingsQuery q, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        return _bookings.GetByUserAsync(userId, ct);
    }
}
