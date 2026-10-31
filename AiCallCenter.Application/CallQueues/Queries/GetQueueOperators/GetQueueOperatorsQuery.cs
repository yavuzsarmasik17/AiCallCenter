using AiCallCenter.Application.CallQueues.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueues.Queries
    .GetQueueOperators;

public sealed record GetQueueOperatorsQuery(
    Guid CompanyId,
    Guid CallQueueId)
    : IRequest<IReadOnlyList<CallQueueOperatorDto>>;