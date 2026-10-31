using MediatR;

namespace AiCallCenter.Application.KnowledgeBases.Commands.DeleteKnowledgeBase;

public sealed record DeleteKnowledgeBaseCommand(
    Guid CompanyId,
    Guid KnowledgeBaseId)
    : IRequest;