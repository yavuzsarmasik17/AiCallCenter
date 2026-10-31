using FluentValidation;

namespace AiCallCenter.Application.AiAgents.Commands.UpdateAiAgentSettings;

public sealed class UpdateAiAgentSettingsCommandValidator
    : AbstractValidator<UpdateAiAgentSettingsCommand>
{
    public UpdateAiAgentSettingsCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Firma kimliği zorunludur.");

        RuleFor(x => x.AiAgentId)
            .NotEmpty()
            .WithMessage("Yapay zekâ ajanı kimliği zorunludur.");

        RuleFor(x => x.VoiceName)
            .NotEmpty()
            .WithMessage("Ses adı zorunludur.")
            .MaximumLength(100)
            .WithMessage("Ses adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.MaxCallDurationMinutes)
            .InclusiveBetween(1, 120)
            .WithMessage(
                "Maksimum görüşme süresi 1 ile 120 dakika arasında olmalıdır.");
    }
}