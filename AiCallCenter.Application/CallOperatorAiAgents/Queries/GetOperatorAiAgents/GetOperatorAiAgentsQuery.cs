using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Queries
    .GetOperatorAiAgents;

public sealed record GetOperatorAiAgentsQuery(
    Guid CompanyId,
    Guid CallOperatorId)
    : IRequest<IReadOnlyList<CallOperatorAiAgentDto>>;