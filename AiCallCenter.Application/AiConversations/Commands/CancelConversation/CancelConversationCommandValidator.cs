using FluentValidation;

namespace AiCallCenter.Application.AiConversations.Commands.CancelConversation;

public sealed class CancelConversationCommandValidator
    : AbstractValidator<CancelConversationCommand>
{
    public CancelConversationCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Sohbet kimliği zorunludur.");
    }
}