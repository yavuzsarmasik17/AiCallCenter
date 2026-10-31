using System.Text;
using AiCallCenter.Application.Common.AI.Handoff;

namespace AiCallCenter.Infrastructure.AI.Handoff;

public sealed class MockAiHandoffSummaryService
    : IAiHandoffSummaryService
{
    private const int MaxSummaryLength = 4000;

    public Task<AiHandoffSummaryResult> CreateAsync(
        AiHandoffSummaryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        var customerMessage =
            Normalize(request.CustomerMessage);

        var transferReason =
            Normalize(request.TransferReason);

        var conversationHistory =
            request.ConversationHistory ??
            Array.Empty<string>();

        var knowledgeResults =
            request.KnowledgeResults ??
            Array.Empty<string>();

        var builder = new StringBuilder();

        builder.AppendLine("AI DEVİR ÖZETİ");
        builder.AppendLine();

        builder.AppendLine("Son müşteri talebi:");
        builder.AppendLine(
            string.IsNullOrWhiteSpace(customerMessage)
                ? "Belirtilmedi."
                : customerMessage);

        builder.AppendLine();

        builder.AppendLine("Aktarım nedeni:");
        builder.AppendLine(
            string.IsNullOrWhiteSpace(transferReason)
                ? "Belirtilmedi."
                : transferReason);

        builder.AppendLine();

        builder.AppendLine("Görüşme durumu:");

        if (conversationHistory.Count == 0)
        {
            builder.AppendLine(
                "Aktarım öncesinde ek görüşme geçmişi bulunmuyor.");
        }
        else
        {
            builder.AppendLine(
                $"Aktarım öncesinde {conversationHistory.Count} mesajlık görüşme geçmişi bulunuyor.");
        }

        builder.AppendLine();

        builder.AppendLine("Bilgi tabanı kontrolü:");

        if (knowledgeResults.Count == 0)
        {
            builder.AppendLine(
                "Müşteri talebi için uygun bilgi tabanı sonucu bulunamadı.");
        }
        else
        {
            builder.AppendLine(
                $"{knowledgeResults.Count} bilgi tabanı sonucu değerlendirildi.");
        }

        builder.AppendLine();

        builder.AppendLine("Operatör önerisi:");
        builder.AppendLine(
            BuildOperatorSuggestion(
                transferReason,
                knowledgeResults.Count));

        var summary = builder
            .ToString()
            .Trim();

        if (summary.Length > MaxSummaryLength)
        {
            summary =
                summary[..MaxSummaryLength];
        }

        return Task.FromResult(
            new AiHandoffSummaryResult(
                summary));
    }

    private static string BuildOperatorSuggestion(
        string transferReason,
        int knowledgeResultCount)
    {
        if (transferReason.Contains(
                "insan temsilci",
                StringComparison.OrdinalIgnoreCase))
        {
            return
                "Müşterinin talebini dinleyerek görüşmeye devam edin.";
        }

        if (knowledgeResultCount == 0)
        {
            return
                "Müşterinin talebini doğrulayın ve gerekli şirket sistemlerinden kontrol sağlayın.";
        }

        return
            "Önceki görüşme geçmişini kontrol ederek müşterinin çözülemeyen talebine devam edin.";
    }

    private static string Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }
}