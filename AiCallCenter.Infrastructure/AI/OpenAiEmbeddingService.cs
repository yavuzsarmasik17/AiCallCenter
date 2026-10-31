using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AiCallCenter.Application.Common.AI;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.AI;

public sealed class OpenAiEmbeddingService
    : IEmbeddingService
{
    private const int MockVectorSize = 1536;

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiEmbeddingService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }
    public string ModelName => _options.EmbeddingModel;

    public async Task<float[]> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Embedding oluşturulacak metin boş olamaz.",
                nameof(text));
        }

        if (_options.UseMockService)
        {
            return CreateMockEmbedding(text);
        }

        var requestBody = new
        {
            model = _options.EmbeddingModel,
            input = text.Trim()
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "embeddings");

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

        return ExtractEmbedding(responseJson);
    }

    private static float[] ExtractEmbedding(
        string responseJson)
    {
        using var document =
            JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty(
                "data",
                out var dataElement) ||
            dataElement.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                "OpenAI embedding cevabında veri bulunamadı.");
        }

        var firstItem = dataElement[0];

        if (!firstItem.TryGetProperty(
                "embedding",
                out var embeddingElement))
        {
            throw new InvalidOperationException(
                "OpenAI cevabında embedding alanı bulunamadı.");
        }

        return embeddingElement
            .EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();
    }

    private static float[] CreateMockEmbedding(string text)
    {
        var normalizedText = text
            .Trim()
            .ToLowerInvariant();

        var vector = new float[MockVectorSize];

        var words = normalizedText.Split(
            new[]
            {
                ' ', ',', '.', '?', '!', ';', ':',
                '\r', '\n', '\t'
            },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(word));

            var index = BitConverter.ToUInt32(
                hash,
                0) % MockVectorSize;

            var sign = (hash[4] & 1) == 0
                ? 1f
                : -1f;

            vector[index] += sign;
        }

        Normalize(vector);

        return vector;
    }

    private static void Normalize(float[] vector)
    {
        var sumOfSquares = vector
            .Sum(value => value * value);

        if (sumOfSquares <= 0)
        {
            return;
        }

        var length = MathF.Sqrt(sumOfSquares);

        for (var index = 0;
             index < vector.Length;
             index++)
        {
            vector[index] /= length;
        }
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
                       ?? "OpenAI embedding isteği başarısız oldu.";
            }
        }
        catch (JsonException)
        {
            // JSON olmayan cevaplarda genel hata döndürülür.
        }

        return "OpenAI embedding isteği başarısız oldu.";
    }
}