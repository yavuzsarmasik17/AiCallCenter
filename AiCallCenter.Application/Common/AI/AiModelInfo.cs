using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Common.AI;

public sealed class AiModelInfo
{
    public AiProvider Provider { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool SupportsReasoning { get; set; }

    public bool SupportsVision { get; set; }

    public bool IsRecommended { get; set; }
}