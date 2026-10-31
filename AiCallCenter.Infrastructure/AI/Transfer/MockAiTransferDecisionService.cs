using AiCallCenter.Application.Common.AI.Transfer;

namespace AiCallCenter.Infrastructure.AI.Transfer;

public sealed class MockAiTransferDecisionService
    : IAiTransferDecisionService
{
    public Task<AiTransferDecision> DecideAsync(
        AiTransferDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        var customerMessage =
            request.CustomerMessage?.Trim() ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(customerMessage))
        {
            return Task.FromResult(
                AiTransferDecision.Continue());
        }

        /*
         * Ajan insan aktarımına kapalıysa
         * karar servisi aktarım üretmez.
         *
         * Böylece AI normal şekilde cevap
         * vermeye devam eder.
         */
        if (!request.TransferToHumanEnabled)
        {
            return Task.FromResult(
                AiTransferDecision.Continue());
        }

        var normalized =
            customerMessage.ToLowerInvariant();

        /*
         * 1. Müşteri açıkça insan temsilci
         * istiyor mu?
         *
         * Bu bölüm Mock döneminde kullanılacak.
         * Gerçek AI geldiğinde structured
         * decision ile değiştirilecek.
         */
        var explicitHumanRequests =
            new[]
            {
                "müşteri temsilcisi",
                "müşteri temsilcisine",
                "temsilciye bağlan",
                "temsilciye aktar",
                "temsilciyle görüş",
                "temsilci ile görüş",
                "yetkiliye bağlan",
                "yetkiliye aktar",
                "yetkiliyle görüş",
                "yetkili ile görüş",
                "operatöre bağlan",
                "operatöre aktar",
                "operatörle görüş",
                "operatör ile görüş",
                "insana bağlan",
                "canlı destek"
            };

        if (
            explicitHumanRequests.Any(
                phrase =>
                    normalized.Contains(phrase)))
        {
            return Task.FromResult(
                AiTransferDecision.Transfer(
                    "Müşteri insan temsilci ile görüşmek istedi.",
                    1.0));
        }

        /*
         * 2. Selamlama / teşekkür / kısa sosyal
         * konuşmalar için Knowledge Base
         * gerekmiyor.
         *
         * KB sonucu boş olsa bile AI
         * cevap vermeye devam eder.
         */
        var conversationalMessages =
            new[]
            {
                "merhaba",
                "selam",
                "selamlar",
                "günaydın",
                "iyi günler",
                "iyi akşamlar",
                "teşekkür",
                "teşekkürler",
                "sağ ol",
                "sağol",
                "tamam",
                "anladım",
                "hoşça kal",
                "görüşürüz"
            };

        var isConversational =
            conversationalMessages.Any(
                phrase =>
                    normalized.Contains(phrase));

        if (isConversational)
        {
            return Task.FromResult(
                AiTransferDecision.Continue(
                    0.95));
        }

        /*
         * 3. İş sorusu sorulmuş fakat Knowledge
         * Base hiçbir sonuç üretmemişse
         * Mock dönemde operatöre aktar.
         *
         * Gerçek AI entegrasyonunda sadece
         * sonuç sayısına değil; relevance,
         * confidence ve AI kararına bakacağız.
         */
        if (
            request.KnowledgeResults is null ||
            request.KnowledgeResults.Count == 0)
        {
            return Task.FromResult(
                AiTransferDecision.Transfer(
                    "Bilgi tabanında müşterinin talebini karşılayacak bilgi bulunamadı.",
                    0.90));
        }

        /*
         * 4. KB sonucu bulundu.
         * AI cevap üretmeye devam edebilir.
         */
        return Task.FromResult(
            AiTransferDecision.Continue(
                0.90));
    }
}