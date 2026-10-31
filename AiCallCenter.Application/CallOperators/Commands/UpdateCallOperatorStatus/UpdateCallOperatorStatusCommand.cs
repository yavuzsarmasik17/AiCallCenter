using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .UpdateCallOperatorStatus;

public sealed record UpdateCallOperatorStatusCommand(
    Guid CompanyId,
    Guid OperatorId,
    CallOperatorStatus Status)
    : IRequest<CallOperatorDto>;