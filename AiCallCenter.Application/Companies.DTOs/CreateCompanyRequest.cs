using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Companies.DTOs;

public sealed class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public SubscriptionPlan SubscriptionPlan { get; set; }
        = SubscriptionPlan.Trial;
}