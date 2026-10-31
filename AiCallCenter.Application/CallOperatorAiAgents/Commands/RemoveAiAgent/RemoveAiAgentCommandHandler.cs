using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Commands
    .RemoveAiAgent;

public sealed class RemoveAiAgentCommandHandler
    : IRequestHandler<RemoveAiAgentCommand>
{
    private readonly ICallOperatorAiAgentRepository _repository;

    public RemoveAiAgentCommandHandler(
        ICallOperatorAiAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        RemoveAiAgentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Operatör kimliği geçersiz.");
        }

        if (request.AssignmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Atama kimliği geçersiz.");
        }

        var assignment =
            await _repository.GetByIdAsync(
                request.CompanyId,
                request.CallOperatorId,
                request.AssignmentId,
                cancellationToken);

        if (assignment is null)
        {
            throw new KeyNotFoundException(
                "AI ajanı ataması bulunamadı.");
        }

        _repository.Remove(assignment);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}