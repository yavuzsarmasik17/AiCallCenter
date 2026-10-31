using FluentValidation;

namespace AiCallCenter.Application.AiConversations.Commands.CreateConversation;

public sealed class CreateConversationCommandValidator
    : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.AiAgentId)
            .NotEmpty()
            .WithMessage("AI ajanı kimliği zorunludur.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(200)
            .WithMessage(
                "Müşteri adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.CustomerPhone)
            .MaximumLength(50)
            .WithMessage(
                "Müşteri telefonu en fazla 50 karakter olabilir.");
    }
}