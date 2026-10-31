using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Commands
    .SetPrimaryAiAgent;

public sealed record SetPrimaryAiAgentCommand(
    Guid CompanyId,
    Guid CallOperatorId,
    Guid AssignmentId)
    : IRequest;