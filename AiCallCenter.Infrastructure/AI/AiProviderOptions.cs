namespace AiCallCenter.Infrastructure.AI;

public sealed class AiProviderOptions
{
    public const string SectionName = "AIProviders";

    public ProviderSettings OpenAI { get; set; } = new();

    public ProviderSettings Gemini { get; set; } = new();

    public ProviderSettings Grok { get; set; } = new();

    public ProviderSettings DeepSeek { get; set; } = new();
}

public sealed class ProviderSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKeyEnvironmentVariable { get; set; }
        = string.Empty;

    public bool UseMockService { get; set; } = true;

    public int TimeoutSeconds { get; set; } = 60;
}