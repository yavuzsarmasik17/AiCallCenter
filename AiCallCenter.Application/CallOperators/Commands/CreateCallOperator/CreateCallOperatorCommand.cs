using AiCallCenter.Application.CallOperators.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .CreateCallOperator;

public sealed record CreateCallOperatorCommand(
    Guid CompanyId,
    Guid UserId,
    string DisplayName,
    string? ExtensionNumber,
    int MaxConcurrentCalls,
    bool CanReceiveCalls)
    : IRequest<CallOperatorDto>;