using AiCallCenter.Application.CallQueues.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Queries
    .GetCallQueues;

public sealed record GetCallQueuesQuery(
    Guid CompanyId)
    : IRequest<IReadOnlyList<CallQueueDto>>;