using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Commands
    .RemoveAiAgent;

public sealed record RemoveAiAgentCommand(
    Guid CompanyId,
    Guid CallOperatorId,
    Guid AssignmentId)
    : IRequest;