using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .DeleteCallQueue;

public sealed record DeleteCallQueueCommand(
    Guid CompanyId,
    Guid CallQueueId)
    : IRequest;