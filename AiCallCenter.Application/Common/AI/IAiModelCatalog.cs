using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Common.AI;

public interface IAiModelCatalog
{
    IReadOnlyList<AiModelInfo> GetAll();

    IReadOnlyList<AiModelInfo> GetByProvider(
        AiProvider provider);

    bool IsSupported(
        AiProvider provider,
        string modelName);

    AiModelInfo GetRequired(
        AiProvider provider,
        string modelName);
}