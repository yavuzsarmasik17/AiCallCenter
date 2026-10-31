namespace AiCallCenter.Infrastructure.AI;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public bool UseMockService { get; set; } = true;

    public string BaseUrl { get; set; }
        = "https://api.openai.com/v1/";

    public string Model { get; set; }
        = "gpt-5-mini";

    public string EmbeddingModel { get; set; }
        = "text-embedding-3-small";

    public int TimeoutSeconds { get; set; } = 60;
}