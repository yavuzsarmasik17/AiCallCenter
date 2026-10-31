using FluentValidation;

namespace AiCallCenter.Application.AiAgents.Commands.UpdateAiAgent;

public sealed class UpdateAiAgentCommandValidator
    : AbstractValidator<UpdateAiAgentCommand>
{
    public UpdateAiAgentCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.AiAgentId)
            .NotEmpty()
            .WithMessage("Yapay zekâ ajanı kimliği zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Ajan adı zorunludur.")
            .MaximumLength(150)
            .WithMessage("Ajan adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Instructions)
            .NotEmpty()
            .WithMessage("Ajan talimatları zorunludur.")
            .MaximumLength(20000)
            .WithMessage("Talimatlar en fazla 20000 karakter olabilir.");

        RuleFor(x => x.WelcomeMessage)
            .NotEmpty()
            .WithMessage("Karşılama mesajı zorunludur.")
            .MaximumLength(1000)
            .WithMessage("Karşılama mesajı en fazla 1000 karakter olabilir.");

        RuleFor(x => x.Language)
            .IsInEnum()
            .WithMessage("Ajan dili geçersiz.");
    }
}