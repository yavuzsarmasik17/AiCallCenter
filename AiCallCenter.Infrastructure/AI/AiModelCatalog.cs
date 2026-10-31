using AiCallCenter.Application.Common.AI;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Infrastructure.AI;

public sealed class AiModelCatalog : IAiModelCatalog
{
    private static readonly IReadOnlyList<AiModelInfo> Models =
        new List<AiModelInfo>
        {
            new()
            {
                Provider = AiProvider.OpenAI,
                ModelName = "gpt-5-mini",
                DisplayName = "GPT-5 Mini",
                Description =
                    "Hızlı ve uygun maliyetli genel kullanım modeli.",
                SupportsReasoning = true,
                SupportsVision = true,
                IsRecommended = true
            },
            new()
            {
                Provider = AiProvider.OpenAI,
                ModelName = "gpt-5",
                DisplayName = "GPT-5",
                Description =
                    "Daha karmaşık ve yüksek doğruluk isteyen görevler için.",
                SupportsReasoning = true,
                SupportsVision = true,
                IsRecommended = false
            },
            new()
            {
                Provider = AiProvider.Gemini,
                ModelName = "gemini-2.5-flash",
                DisplayName = "Gemini 2.5 Flash",
                Description =
                    "Hızlı ve düşük gecikmeli Gemini modeli.",
                SupportsReasoning = true,
                SupportsVision = true,
                IsRecommended = true
            },
            new()
            {
                Provider = AiProvider.Gemini,
                ModelName = "gemini-2.5-pro",
                DisplayName = "Gemini 2.5 Pro",
                Description =
                    "Karmaşık görevler için güçlü Gemini modeli.",
                SupportsReasoning = true,
                SupportsVision = true,
                IsRecommended = false
            },
            new()
            {
                Provider = AiProvider.Grok,
                ModelName = "grok-3-mini",
                DisplayName = "Grok 3 Mini",
                Description =
                    "Hızlı ve ekonomik Grok modeli.",
                SupportsReasoning = true,
                SupportsVision = false,
                IsRecommended = true
            },
            new()
            {
                Provider = AiProvider.Grok,
                ModelName = "grok-3",
                DisplayName = "Grok 3",
                Description =
                    "Daha güçlü Grok modeli.",
                SupportsReasoning = true,
                SupportsVision = false,
                IsRecommended = false
            },
            new()
            {
                Provider = AiProvider.DeepSeek,
                ModelName = "deepseek-chat",
                DisplayName = "DeepSeek Chat",
                Description =
                    "Genel sohbet ve müşteri hizmetleri için.",
                SupportsReasoning = false,
                SupportsVision = false,
                IsRecommended = true
            },
            new()
            {
                Provider = AiProvider.DeepSeek,
                ModelName = "deepseek-reasoner",
                DisplayName = "DeepSeek Reasoner",
                Description =
                    "Muhakeme gerektiren görevler için.",
                SupportsReasoning = true,
                SupportsVision = false,
                IsRecommended = false
            }
        };

    public IReadOnlyList<AiModelInfo> GetAll()
    {
        return Models;
    }

    public IReadOnlyList<AiModelInfo> GetByProvider(
        AiProvider provider)
    {
        return Models
            .Where(model => model.Provider == provider)
            .ToList();
    }

    public bool IsSupported(
        AiProvider provider,
        string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return false;
        }

        return Models.Any(model =>
            model.Provider == provider &&
            string.Equals(
                model.ModelName,
                modelName.Trim(),
                StringComparison.OrdinalIgnoreCase));
    }

    public AiModelInfo GetRequired(
        AiProvider provider,
        string modelName)
    {
        var model = Models.FirstOrDefault(model =>
            model.Provider == provider &&
            string.Equals(
                model.ModelName,
                modelName.Trim(),
                StringComparison.OrdinalIgnoreCase));

        return model
            ?? throw new InvalidOperationException(
                "Seçilen sağlayıcı ve model desteklenmiyor.");
    }
}