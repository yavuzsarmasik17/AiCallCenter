using FluentValidation;

namespace AiCallCenter.Application.AiAgents.Commands.DetachKnowledgeBase;

public sealed class DetachKnowledgeBaseCommandValidator
    : AbstractValidator<DetachKnowledgeBaseCommand>
{
    public DetachKnowledgeBaseCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.AiAgentId)
            .NotEmpty()
            .WithMessage("Yapay zekâ ajanı kimliği zorunludur.");

        RuleFor(x => x.KnowledgeBaseId)
            .NotEmpty()
            .WithMessage("Bilgi bankası kimliği zorunludur.");
    }
}