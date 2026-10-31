using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.AttachKnowledgeBase;

public sealed class AttachKnowledgeBaseCommandHandler
    : IRequestHandler<AttachKnowledgeBaseCommand>
{
    private readonly IAiAgentKnowledgeBaseRepository _repository;

    public AttachKnowledgeBaseCommandHandler(
        IAiAgentKnowledgeBaseRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        AttachKnowledgeBaseCommand request,
        CancellationToken cancellationToken)
    {
        var aiAgentExists = await _repository.AiAgentExistsAsync(
            request.CompanyId,
            request.AiAgentId,
            cancellationToken);

        if (!aiAgentExists)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }

        var knowledgeBaseExists =
            await _repository.KnowledgeBaseExistsAsync(
                request.CompanyId,
                request.KnowledgeBaseId,
                cancellationToken);

        if (!knowledgeBaseExists)
        {
            throw new KeyNotFoundException(
                "Bilgi bankası bulunamadı.");
        }

        var connectionExists =
            await _repository.ConnectionExistsAsync(
                request.AiAgentId,
                request.KnowledgeBaseId,
                cancellationToken);

        if (connectionExists)
        {
            throw new InvalidOperationException(
                "Bu bilgi bankası zaten yapay zekâ ajanına bağlı.");
        }

        var connection = new AiAgentKnowledgeBase(
            request.AiAgentId,
            request.KnowledgeBaseId);

        await _repository.AddAsync(
            connection,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}