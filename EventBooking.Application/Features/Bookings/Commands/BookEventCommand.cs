using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace EventBooking.Application.Features.Bookings.Commands;

public record BookEventCommand(Guid EventId) : IRequest<BookingDto>;

public class BookEventCommandValidator : AbstractValidator<BookEventCommand>
{
    public BookEventCommandValidator() => RuleFor(x => x.EventId).NotEmpty();
}

public class BookEventCommandHandler : IRequestHandler<BookEventCommand, BookingDto>
{
    private readonly IEventRepository _events;
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public BookEventCommandHandler(IEventRepository events, IBookingRepository bookings,
                                   IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _events = events;
        _bookings = bookings;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<BookingDto> Handle(BookEventCommand r, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        var ev = await _events.GetByIdWithBookingsAsync(r.EventId, ct)
                 ?? throw new NotFoundException("Event", r.EventId);

        var booking = ev.Book(userId);   // capacity / duplicate / date rules are enforced inside the entity
        await _uow.SaveChangesAsync(ct); // RowVersion + unique index protect against races

        return await _bookings.GetDtoByIdAsync(booking.Id, ct)
               ?? throw new NotFoundException("Booking", booking.Id);
    }
}
