using EventBooking.Application.Common.Exceptions;
using EventBooking.Application.Common.Interfaces;
using MediatR;

namespace EventBooking.Application.Features.Events.Queries;

public record GetEventByIdQuery(Guid Id) : IRequest<EventDto>;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IEventRepository _events;

    public GetEventByIdQueryHandler(IEventRepository events) => _events = events;

    public async Task<EventDto> Handle(GetEventByIdQuery q, CancellationToken ct)
    {
        var ev = await _events.GetByIdWithBookingsAsync(q.Id, ct)
                 ?? throw new NotFoundException("Event", q.Id);

        return ev.ToDto();
    }
}
