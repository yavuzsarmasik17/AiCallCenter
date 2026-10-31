using AiCallCenter.Application.CallQueueItems.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallQueueItems.Commands.EnqueueCall;

public sealed record EnqueueCallCommand(
    Guid CompanyId,
    Guid CallQueueId,
    Guid CallId,
    int? Priority,
    string? TransferReason,
    string? AiHandoffSummary = null)
    : IRequest<EnqueueCallResultDto>;