using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class KnowledgeBase : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;

    public ICollection<KnowledgeDocument> Documents
    { get; private set; }
        = new List<KnowledgeDocument>();

    public ICollection<AiAgentKnowledgeBase> AiAgents
    { get; private set; }
    = new List<AiAgentKnowledgeBase>();

    private KnowledgeBase()
    {
    }

    public KnowledgeBase(
        Guid companyId,
        string name,
        string? description = null)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        CompanyId = companyId;

        SetName(name);

        Description = NormalizeOptionalValue(description);
        IsActive = true;
    }

    public void Update(
        string name,
        string? description)
    {
        SetName(name);
        Description = NormalizeOptionalValue(description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Bilgi bankası adı boş olamaz.",
                nameof(name));
        }

        if (name.Trim().Length > 200)
        {
            throw new ArgumentException(
                "Bilgi bankası adı en fazla 200 karakter olabilir.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}