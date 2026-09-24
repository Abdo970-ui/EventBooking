using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using MediatR;

namespace EventBooking.Application.Features.Bookings.Queries;

public record GetEventBookingsQuery(Guid EventId) : IRequest<IReadOnlyList<BookingDto>>;

public class GetEventBookingsQueryHandler : IRequestHandler<GetEventBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IEventRepository _events;
    private readonly IBookingRepository _bookings;
    private readonly ICurrentUserService _currentUser;

    public GetEventBookingsQueryHandler(IEventRepository events, IBookingRepository bookings, ICurrentUserService currentUser)
    {
        _events = events;
        _bookings = bookings;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BookingDto>> Handle(GetEventBookingsQuery q, CancellationToken ct)
    {
        var ev = await _events.GetByIdWithBookingsAsync(q.EventId, ct)
                 ?? throw new NotFoundException("Event", q.EventId);

        if (ev.OrganizerId != _currentUser.UserId)
            throw new ForbiddenException("Only the organizer of this event can see its bookings.");

        return await _bookings.GetByEventAsync(q.EventId, ct);
    }
}
