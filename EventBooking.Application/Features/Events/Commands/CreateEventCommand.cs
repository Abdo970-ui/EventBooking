using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using EventBooking.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EventBooking.Application.Features.Events.Commands;

public record CreateEventCommand(string Title, string? Description, string Location, DateTime Date, int Capacity)
    : IRequest<EventDto>;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 100_000);
        RuleFor(x => x.Date).Must(d => d > DateTime.UtcNow).WithMessage("Event date must be in the future.");
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateEventCommandHandler(IEventRepository events, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _events = events;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EventDto> Handle(CreateEventCommand r, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        var ev = Event.Create(r.Title, r.Description, r.Location, r.Date, r.Capacity, userId);

        await _events.AddAsync(ev, ct);
        await _uow.SaveChangesAsync(ct);

        return ev.ToDto();
    }
}
