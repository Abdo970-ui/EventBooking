using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace EventBooking.Application.Features.Events.Commands;

public record UpdateEventCommand(Guid Id, string Title, string? Description, string Location, DateTime Date, int Capacity)
    : IRequest<EventDto>;

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 100_000);
        RuleFor(x => x.Date).Must(d => d > DateTime.UtcNow).WithMessage("Event date must be in the future.");
    }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventDto>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateEventCommandHandler(IEventRepository events, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _events = events;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EventDto> Handle(UpdateEventCommand r, CancellationToken ct)
    {
        var ev = await _events.GetByIdWithBookingsAsync(r.Id, ct)
                 ?? throw new NotFoundException("Event", r.Id);

        if (ev.OrganizerId != _currentUser.UserId)
            throw new ForbiddenException("Only the organizer of this event can modify it.");

        ev.Update(r.Title, r.Description, r.Location, r.Date, r.Capacity); // business rules live in the entity
        await _uow.SaveChangesAsync(ct);

        return ev.ToDto();
    }
}
