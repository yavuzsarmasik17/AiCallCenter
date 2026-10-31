using System.Text;
using System.Text.Json;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.AI;

public sealed class OpenAiService
    : IOpenAiService,
      IAiProviderClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public AiProvider Provider => AiProvider.OpenAI;

    // Eski kodların çalışmaya devam etmesi için korunuyor.
    public Task<string> AskAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        return AskAsync(
            _options.Model,
            systemPrompt,
            userMessage,
            cancellationToken);
    }

    // Yeni çoklu sağlayıcı altyapısının kullandığı metot.
    public async Task<string> AskAsync(
        string modelName,
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException(
                "Model adı boş olamaz.",
                nameof(modelName));
        }

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException(
                "Kullanıcı mesajı boş olamaz.",
                nameof(userMessage));
        }

        if (_options.UseMockService)
        {
            return CreateMockResponse(
                modelName,
                systemPrompt,
                userMessage);
        }

        var requestBody = new
        {
            model = modelName.Trim(),
            instructions = systemPrompt,
            input = userMessage.Trim()
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "responses");

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

        return ExtractOutputText(responseJson);
    }

    private static string CreateMockResponse(
        string modelName,
        string systemPrompt,
        string message)
    {
        var normalizedMessage = message
            .Trim()
            .ToLowerInvariant();

        if (normalizedMessage.Contains("çalışma saat"))
        {
            if (systemPrompt.Contains(
                    "09.00 ile 18.00",
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Firmamız hafta içi 09.00 ile 18.00 arasında hizmet vermektedir.";
            }

            return
                "Çalışma saatleri hakkında bilgi bankasında kayıt bulunamadı.";
        }

        if (normalizedMessage.Contains("cumartesi"))
        {
            if (systemPrompt.Contains(
                    "09.00 ile 14.00",
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Cumartesi günü 09.00 ile 14.00 arasında hizmet verilmektedir.";
            }

            return
                "Cumartesi çalışma saatleri hakkında bilgi bulunamadı.";
        }

        if (normalizedMessage.Contains("pazar"))
        {
            if (systemPrompt.Contains(
                    "pazar günü kapalı",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Firmamız pazar günü kapalıdır.";
            }

            return
                "Pazar günü çalışma durumu hakkında bilgi bulunamadı.";
        }

        if (normalizedMessage.Contains("merhaba"))
        {
            return
                $"Merhaba, size nasıl yardımcı olabilirim? " +
                $"Test modeli: {modelName}";
        }

        if (normalizedMessage.Contains("temsilci"))
        {
            return
                "Elbette, sizi müşteri temsilcisine aktarıyorum.";
        }

        return systemPrompt.Contains(
            "BİLGİ BANKASI İÇERİĞİ:",
            StringComparison.OrdinalIgnoreCase)
            ? "Bilgi bankasında ilgili içerik bulundu ancak test modunda bu soru için özel bir cevap tanımlanmadı."
            : "Test modu aktif. Sorunuz için bilgi bankasında uygun kayıt bulunamadı.";
    }

    private static string ExtractOutputText(
        string responseJson)
    {
        using var document =
            JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty(
                "output",
                out var outputArray))
        {
            throw new InvalidOperationException(
                "OpenAI cevabında output alanı bulunamadı.");
        }

        foreach (var outputItem in outputArray.EnumerateArray())
        {
            if (!outputItem.TryGetProperty(
                    "content",
                    out var contentArray))
            {
                continue;
            }

            foreach (var contentItem in contentArray.EnumerateArray())
            {
                if (!contentItem.TryGetProperty(
                        "type",
                        out var typeElement))
                {
                    continue;
                }

                var type = typeElement.GetString();

                if (type == "output_text" &&
                    contentItem.TryGetProperty(
                        "text",
                        out var textElement))
                {
                    return textElement.GetString()
                           ?? "Yapay zekâ boş cevap döndürdü.";
                }
            }
        }

        throw new InvalidOperationException(
            "OpenAI tarafından metin cevabı döndürülmedi.");
    }

    private static string GetErrorMessage(
        string responseJson)
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
                       ?? "OpenAI API hatası oluştu.";
            }
        }
        catch (JsonException)
        {
            // Cevap JSON değilse genel hata döndürülür.
        }

        return "OpenAI API isteği başarısız oldu.";
    }
}