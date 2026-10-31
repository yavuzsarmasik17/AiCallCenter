using FluentValidation;

namespace AiCallCenter.Application.AiAgents.Commands.AttachKnowledgeBase;

public sealed class AttachKnowledgeBaseCommandValidator
    : AbstractValidator<AttachKnowledgeBaseCommand>
{
    public AttachKnowledgeBaseCommandValidator()
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