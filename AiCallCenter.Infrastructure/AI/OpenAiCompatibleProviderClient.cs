using System.Text;
using System.Text.Json;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Infrastructure.AI;

public abstract class OpenAiCompatibleProviderClient
    : IAiProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly ProviderSettings _settings;

    protected OpenAiCompatibleProviderClient(
        HttpClient httpClient,
        ProviderSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public abstract AiProvider Provider { get; }

    protected abstract string ProviderName { get; }

    public async Task<string> AskAsync(
        string modelName,
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException(
                $"{ProviderName} model adı boş olamaz.",
                nameof(modelName));
        }

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException(
                "Kullanıcı mesajı boş olamaz.",
                nameof(userMessage));
        }

        if (_settings.UseMockService)
        {
            return CreateMockResponse(
                modelName,
                systemPrompt,
                userMessage);
        }

        var requestBody = new
        {
            model = modelName.Trim(),

            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = userMessage.Trim()
                }
            },

            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseJson =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                GetErrorMessage(responseJson));
        }

        return ExtractMessage(responseJson);
    }

    private string CreateMockResponse(
        string modelName,
        string systemPrompt,
        string userMessage)
    {
        var knowledgeFound = systemPrompt.Contains(
            "BİLGİ BANKASI İÇERİĞİ:",
            StringComparison.OrdinalIgnoreCase);

        return knowledgeFound
            ? $"{ProviderName} test modu aktif. " +
              $"Model: {modelName}. " +
              $"Bilgi bankası kullanılarak mesaj alındı: " +
              $"{userMessage.Trim()}"
            : $"{ProviderName} test modu aktif. " +
              $"Model: {modelName}. " +
              $"Mesajınız alındı: {userMessage.Trim()}";
    }

    private string ExtractMessage(string responseJson)
    {
        using var document =
            JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty(
                "choices",
                out var choices) ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"{ProviderName} cevabında choices alanı bulunamadı.");
        }

        var firstChoice = choices[0];

        if (!firstChoice.TryGetProperty(
                "message",
                out var messageElement) ||
            !messageElement.TryGetProperty(
                "content",
                out var contentElement))
        {
            throw new InvalidOperationException(
                $"{ProviderName} cevabında mesaj içeriği bulunamadı.");
        }

        return contentElement.GetString()
               ?? $"{ProviderName} boş cevap döndürdü.";
    }

    private string GetErrorMessage(string responseJson)
    {
        try
        {
            using var document =
                JsonDocument.Parse(responseJson);

            if (document.RootElement.TryGetProperty(
                    "error",
                    out var errorElement) &&
                errorElement.TryGetProperty(
                    "message",
                    out var messageElement))
            {
                return messageElement.GetString()
                       ?? $"{ProviderName} API hatası oluştu.";
            }
        }
        catch (JsonException)
        {
            // JSON olmayan cevaplarda genel hata döndürülür.
        }

        return $"{ProviderName} API isteği başarısız oldu.";
    }
}