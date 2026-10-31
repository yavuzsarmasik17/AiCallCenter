using AiCallCenter.Application.CallRouting.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallRouting.Commands
    .RouteWaitingCall;

public sealed record RouteWaitingCallCommand(
    Guid CompanyId,
    Guid QueueItemId)
    : IRequest<CallRoutingResultDto>;
