using MediatR;

namespace AiCallCenter.Application.CallQueues.Commands
    .RemoveOperatorFromQueue;

public sealed record RemoveOperatorFromQueueCommand(
    Guid CompanyId,
    Guid CallQueueId,
    Guid AssignmentId)
    : IRequest;