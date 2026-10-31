using AiCallCenter.Application.CallQueues.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Queries
    .GetCallQueueById;

public sealed record GetCallQueueByIdQuery(
    Guid CompanyId,
    Guid CallQueueId)
    : IRequest<CallQueueDto>;