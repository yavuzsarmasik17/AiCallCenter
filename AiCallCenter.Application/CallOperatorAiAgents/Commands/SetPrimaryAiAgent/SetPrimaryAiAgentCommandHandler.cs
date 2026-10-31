using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Commands
    .SetPrimaryAiAgent;

public sealed class SetPrimaryAiAgentCommandHandler
    : IRequestHandler<SetPrimaryAiAgentCommand>
{
    private readonly ICallOperatorAiAgentRepository _repository;

    public SetPrimaryAiAgentCommandHandler(
        ICallOperatorAiAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(
        SetPrimaryAiAgentCommand request,
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

        var currentPrimary =
            await _repository.GetPrimaryAsync(
                request.CompanyId,
                request.CallOperatorId,
                cancellationToken);

        if (currentPrimary is not null &&
            currentPrimary.Id != assignment.Id)
        {
            currentPrimary.RemovePrimary();
        }

        assignment.SetAsPrimary();

        await _repository.SaveChangesAsync(
            cancellationToken);
    }
}