using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using MediatR;

namespace EventBooking.Application.Features.Events.Commands;

public record CancelEventCommand(Guid Id) : IRequest;

public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CancelEventCommandHandler(IEventRepository events, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _events = events;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelEventCommand r, CancellationToken ct)
    {
        var ev = await _events.GetByIdWithBookingsAsync(r.Id, ct)
                 ?? throw new NotFoundException("Event", r.Id);

        if (ev.OrganizerId != _currentUser.UserId)
            throw new ForbiddenException("Only the organizer of this event can cancel it.");

        ev.Cancel();
        await _uow.SaveChangesAsync(ct);
    }
}
