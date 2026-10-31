using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Companies.DTOs;

public sealed class CompanyDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? TaxNumber { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public SubscriptionPlan SubscriptionPlan { get; set; }

    public int MaxUserCount { get; set; }

    public int MaxAiAgentCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}