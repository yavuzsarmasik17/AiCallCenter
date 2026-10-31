using AiCallCenter.Application.AiAgents.DTOs;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Queries.GetAiAgents;

public sealed record GetAiAgentsQuery(Guid CompanyId)
    : IRequest<IReadOnlyList<AiAgentDto>>;