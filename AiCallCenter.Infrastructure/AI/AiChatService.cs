using AiCallCenter.Application.Common.AI;

namespace AiCallCenter.Infrastructure.AI;

public sealed class AiChatService : IAiChatService
{
    private readonly IReadOnlyDictionary<
        Domain.Enums.AiProvider,
        IAiProviderClient> _providerClients;

    public AiChatService(
        IEnumerable<IAiProviderClient> providerClients)
    {
        _providerClients = providerClients
            .GroupBy(client => client.Provider)
            .ToDictionary(
                group => group.Key,
                group => group.First());
    }

    public Task<string> AskAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.ModelName))
        {
            throw new ArgumentException(
                "Model adı boş olamaz.",
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            throw new ArgumentException(
                "Kullanıcı mesajı boş olamaz.",
                nameof(request));
        }

        if (!_providerClients.TryGetValue(
                request.Provider,
                out var providerClient))
        {
            throw new InvalidOperationException(
                $"Seçilen AI sağlayıcısı kayıtlı değil: " +
                $"{request.Provider}");
        }

        return providerClient.AskAsync(
            request.ModelName,
            request.SystemPrompt,
            request.UserMessage,
            cancellationToken);
    }
}