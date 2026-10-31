using AiCallCenter.Application.AiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.DeleteAiAgent;

public sealed class DeleteAiAgentCommandHandler
    : IRequestHandler<DeleteAiAgentCommand>
{
    private readonly IAiAgentRepository
        _aiAgentRepository;

    public DeleteAiAgentCommandHandler(
        IAiAgentRepository aiAgentRepository)
    {
        _aiAgentRepository =
            aiAgentRepository;
    }

    public async Task Handle(
        DeleteAiAgentCommand request,
        CancellationToken cancellationToken)
    {
        var deleted =
            await _aiAgentRepository
                .DeleteWithRelatedDataAsync(
                    request.CompanyId,
                    request.AiAgentId,
                    cancellationToken);

        if (!deleted)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }
    }
}