using AiCallCenter.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.AI;

public sealed class GrokProviderClient
    : OpenAiCompatibleProviderClient
{
    public GrokProviderClient(
        HttpClient httpClient,
        IOptions<AiProviderOptions> options)
        : base(
            httpClient,
            options.Value.Grok)
    {
    }

    public override AiProvider Provider =>
        AiProvider.Grok;

    protected override string ProviderName =>
        "Grok";
}