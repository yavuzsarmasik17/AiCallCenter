using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Queries
    .GetAvailableAiAgents;

public sealed record GetAvailableAiAgentsQuery(
    Guid CompanyId,
    Guid CallOperatorId)
    : IRequest<IReadOnlyList<AvailableAiAgentDto>>;