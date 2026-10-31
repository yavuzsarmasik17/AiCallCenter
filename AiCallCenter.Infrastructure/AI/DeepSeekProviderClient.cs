using AiCallCenter.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.AI;

public sealed class DeepSeekProviderClient
    : OpenAiCompatibleProviderClient
{
    public DeepSeekProviderClient(
        HttpClient httpClient,
        IOptions<AiProviderOptions> options)
        : base(
            httpClient,
            options.Value.DeepSeek)
    {
    }

    public override AiProvider Provider =>
        AiProvider.DeepSeek;

    protected override string ProviderName =>
        "DeepSeek";
}