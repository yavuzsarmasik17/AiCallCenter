using FluentValidation;

namespace AiCallCenter.Application.AiAgents.Commands.ChatWithAiAgent;

public sealed class ChatWithAiAgentCommandValidator
    : AbstractValidator<ChatWithAiAgentCommand>
{
    public ChatWithAiAgentCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.AiAgentId)
            .NotEmpty()
            .WithMessage("Yapay zekâ ajanı kimliği zorunludur.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Mesaj boş olamaz.")
            .MaximumLength(4000)
            .WithMessage("Mesaj en fazla 4000 karakter olabilir.");
    }
}