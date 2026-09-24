using EventBooking.Application.Common.Interfaces;
using EventBooking.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace EventBooking.Application.Features.Events.Queries;

// Init properties (not positional) so [FromQuery] model binding + defaults work reliably.
public record GetEventsQuery : IRequest<PagedResult<EventDto>>
{
    public string? Location { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetEventsQueryValidator : AbstractValidator<GetEventsQuery>
{
    public GetEventsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("'From' must be earlier than or equal to 'To'.");
    }
}

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PagedResult<EventDto>>
{
    private readonly IEventRepository _events;

    public GetEventsQueryHandler(IEventRepository events) => _events = events;

    public Task<PagedResult<EventDto>> Handle(GetEventsQuery q, CancellationToken ct) =>
        _events.GetPagedAsync(q.Location, q.From, q.To, q.Page, q.PageSize, ct);
}
