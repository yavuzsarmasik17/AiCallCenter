using AiCallCenter.Application.AiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.DetachKnowledgeBase;

public sealed class DetachKnowledgeBaseCommandHandler
    : IRequestHandler<DetachKnowledgeBaseCommand>
{
    private readonly IAiAgentKnowledgeBaseRepository _repository;

    public DetachKnowledgeBaseCommandHandler(
        IAiAgentKnowledgeBaseRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        DetachKnowledgeBaseCommand request,
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

        var connection = await _repository.GetConnectionAsync(
            request.AiAgentId,
            request.KnowledgeBaseId,
            cancellationToken);

        if (connection is null)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı ile bilgi bankası arasında bağlantı bulunamadı.");
        }

        _repository.Remove(connection);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}