using AiCallCenter.Application.CallOperators.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .UpdateCallOperator;

public sealed record UpdateCallOperatorCommand(
    Guid CompanyId,
    Guid OperatorId,
    string DisplayName,
    string? ExtensionNumber,
    int MaxConcurrentCalls,
    bool CanReceiveCalls)
    : IRequest<CallOperatorDto>;