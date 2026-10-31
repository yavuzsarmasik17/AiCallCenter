using FluentValidation;

namespace AiCallCenter.Application.AiAgents.Commands.DeleteAiAgent;

public sealed class DeleteAiAgentCommandValidator
    : AbstractValidator<DeleteAiAgentCommand>
{
    public DeleteAiAgentCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.AiAgentId)
            .NotEmpty()
            .WithMessage("AI ajanı kimliği zorunludur.");
    }
}