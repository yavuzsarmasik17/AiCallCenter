using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class Company : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? TaxNumber { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Email { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public SubscriptionPlan SubscriptionPlan { get; private set; }

    public int MaxUserCount { get; private set; }

    public int MaxAiAgentCount { get; private set; }

    public bool IsActive { get; private set; }

    public ICollection<AiAgent> AiAgents { get; private set; }
    = new List<AiAgent>();

    public ICollection<Call> Calls { get; private set; }
    = new List<Call>();

    public ICollection<AiConversation> Conversations
    { get; private set; }
    = new List<AiConversation>();

    public ICollection<KnowledgeBase> KnowledgeBases
    { get; private set; }
    = new List<KnowledgeBase>();

    public ICollection<User> Users { get; private set; }
    = new List<User>();


    private Company()
    {
    }

    public Company(
        string name,
        string slug,
        SubscriptionPlan subscriptionPlan = SubscriptionPlan.Trial)
    {
        SetName(name);
        SetSlug(slug);

        SubscriptionPlan = subscriptionPlan;
        MaxUserCount = 3;
        MaxAiAgentCount = 1;
        IsActive = true;
    }

    public void UpdateContactInformation(
        string? taxNumber,
        string? phoneNumber,
        string? email)
    {
        TaxNumber = NormalizeOptionalValue(taxNumber);
        PhoneNumber = NormalizeOptionalValue(phoneNumber);
        Email = NormalizeOptionalValue(email);
    }

    public void ChangeSubscription(
        SubscriptionPlan subscriptionPlan,
        int maxUserCount,
        int maxAiAgentCount)
    {
        if (maxUserCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxUserCount),
                "Maksimum kullanıcı sayısı sıfırdan büyük olmalıdır.");
        }

        if (maxAiAgentCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAiAgentCount),
                "Maksimum yapay zekâ ajanı sayısı sıfırdan büyük olmalıdır.");
        }

        SubscriptionPlan = subscriptionPlan;
        MaxUserCount = maxUserCount;
        MaxAiAgentCount = maxAiAgentCount;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Rename(string name)
    {
        SetName(name);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Firma adı boş olamaz.",
                nameof(name));
        }

        if (name.Length > 200)
        {
            throw new ArgumentException(
                "Firma adı en fazla 200 karakter olabilir.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException(
                "Firma kısa adı boş olamaz.",
                nameof(slug));
        }

        Slug = slug
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}