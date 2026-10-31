using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Commands
    .AssignAiAgent;

public sealed record AssignAiAgentCommand(
    Guid CompanyId,
    Guid CallOperatorId,
    Guid AiAgentId,
    bool IsPrimary)
    : IRequest<CallOperatorAiAgentDto>;