using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using MediatR;

namespace AiCallCenter.Application.KnowledgeDocuments.Commands.ProcessEmbeddings;

public sealed record ProcessEmbeddingsCommand(
    Guid CompanyId,
    Guid KnowledgeBaseId,
    int MaximumCount)
    : IRequest<EmbeddingProcessResultDto>;