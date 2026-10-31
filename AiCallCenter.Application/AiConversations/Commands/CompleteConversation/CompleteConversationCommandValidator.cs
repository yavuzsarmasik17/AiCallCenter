using FluentValidation;

namespace AiCallCenter.Application.AiConversations.Commands.CompleteConversation;

public sealed class CompleteConversationCommandValidator
    : AbstractValidator<CompleteConversationCommand>
{
    public CompleteConversationCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Sohbet kimliği zorunludur.");
    }
}