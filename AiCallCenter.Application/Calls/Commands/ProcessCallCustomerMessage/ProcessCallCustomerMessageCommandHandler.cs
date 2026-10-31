using System.Text;
using AiCallCenter.Application.CallQueueItems.Commands.EnqueueCall;
using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Application.Common.AI.Handoff;
using AiCallCenter.Application.Common.AI.Transfer;
using AiCallCenter.Application.Common.Knowledge;
using AiCallCenter.Application.Common.Realtime;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Calls.Commands.ProcessCallCustomerMessage;

public sealed class ProcessCallCustomerMessageCommandHandler
    : IRequestHandler<
        ProcessCallCustomerMessageCommand,
        ProcessCallCustomerMessageResponse>
{
    private readonly ICallRepository _callRepository;
    private readonly ICallMessageRepository _callMessageRepository;
    private readonly IAiChatService _aiChatService;
    private readonly IKnowledgeSearchService _knowledgeSearchService;
    private readonly IAiTransferDecisionService _transferDecisionService;
    private readonly IAiHandoffSummaryService _handoffSummaryService;
    private readonly ICallRealtimeNotifier _realtimeNotifier;
    private readonly ISender _sender;

    public ProcessCallCustomerMessageCommandHandler(
        ICallRepository callRepository,
        ICallMessageRepository callMessageRepository,
        IAiChatService aiChatService,
        IKnowledgeSearchService knowledgeSearchService,
        IAiTransferDecisionService transferDecisionService,
        IAiHandoffSummaryService handoffSummaryService,
        ICallRealtimeNotifier realtimeNotifier,
        ISender sender)
    {
        _callRepository = callRepository;
        _callMessageRepository = callMessageRepository;
        _aiChatService = aiChatService;
        _knowledgeSearchService = knowledgeSearchService;
        _transferDecisionService = transferDecisionService;
        _handoffSummaryService = handoffSummaryService;
        _realtimeNotifier = realtimeNotifier;
        _sender = sender;
    }

    public async Task<ProcessCallCustomerMessageResponse> Handle(
        ProcessCallCustomerMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ArgumentException(
                "Müşteri mesajı boş olamaz.");
        }

        var customerText =
            request.Message.Trim();

        // -------------------------------------------------
        // Çağrıyı yükle
        // -------------------------------------------------

        var call =
            await _callRepository.GetByIdAsync(
                request.CompanyId,
                request.CallId,
                cancellationToken);

        if (call is null)
        {
            throw new KeyNotFoundException(
                "Çağrı bulunamadı.");
        }

        // -------------------------------------------------
        // Önceki konuşma geçmişini yükle
        // -------------------------------------------------

        var previousMessages =
            await _callMessageRepository
                .GetByCallIdAsync(
                    request.CompanyId,
                    request.CallId,
                    cancellationToken);

        // -------------------------------------------------
        // Müşteri mesajını kaydet
        // -------------------------------------------------

        var customerSequenceNumber =
            await _callMessageRepository
                .GetNextSequenceNumberAsync(
                    request.CompanyId,
                    request.CallId,
                    cancellationToken);

        var customerMessage =
            new CallMessage(
                request.CallId,
                MessageSpeaker.Customer,
                customerText,
                customerSequenceNumber);

        await _callMessageRepository.AddAsync(
            customerMessage,
            cancellationToken);

        await _callMessageRepository.SaveChangesAsync(
            cancellationToken);

        await NotifyMessageCreatedAsync(
            request.CompanyId,
            customerMessage,
            cancellationToken);

        // -------------------------------------------------
        // Çağrı daha önce insan operatöre devredildiyse
        // AI artık konuşmaya dahil olmaz.
        //
        // Müşteri mesajı yukarıda CallMessages'a kaydedildi
        // ve realtime notifier ile operatöre bildirildi.
        // -------------------------------------------------

        if (call.TransferredToHuman)
        {
            return new ProcessCallCustomerMessageResponse
            {
                CallId = call.Id,

                CustomerMessage =
                    ToDto(customerMessage),

                AiMessage = null,

                Answer = null,

                HandledByHuman = true
            };
        }

        // -------------------------------------------------
        // Bundan sonrası yalnızca AI tarafından yönetilen
        // çağrılar için çalışır.
        // -------------------------------------------------

        if (call.AiAgent is null)
        {
            throw new InvalidOperationException(
                "Çağrıya bağlı yapay zekâ ajanı bulunamadı.");
        }

        if (!call.AiAgent.IsActive)
        {
            throw new InvalidOperationException(
                "Çağrıya bağlı yapay zekâ ajanı aktif değil.");
        }

        // -------------------------------------------------
        // Knowledge Base arama sorgusunu oluştur
        // -------------------------------------------------

        var knowledgeSearchQuery =
            BuildKnowledgeSearchQuery(
                previousMessages,
                customerText);

        // -------------------------------------------------
        // Knowledge Base içerisinde ara
        // -------------------------------------------------

        var knowledgeResults =
            await _knowledgeSearchService.SearchAsync(
                request.CompanyId,
                call.AiAgentId,
                knowledgeSearchQuery,
                maximumResultCount: 5,
                cancellationToken);

        // -------------------------------------------------
        // AI -> İnsan Operatör kararını ver
        // -------------------------------------------------

        var transferDecision =
            await _transferDecisionService.DecideAsync(
                new AiTransferDecisionRequest
                {
                    CustomerMessage =
                        customerText,

                    TransferToHumanEnabled =
                        call.AiAgent
                            .TransferToHumanEnabled,

                    KnowledgeResults =
                        knowledgeResults
                },
                cancellationToken);

        // -------------------------------------------------
        // İnsan operatöre aktarılması gerekiyorsa
        // -------------------------------------------------

        if (transferDecision.ShouldTransfer)
        {
            return await ProcessHumanTransferAsync(
                request,
                call,
                customerMessage,
                previousMessages,
                knowledgeResults,
                transferDecision,
                cancellationToken);
        }

        // -------------------------------------------------
        // AI sistem promptunu oluştur
        // -------------------------------------------------

        var systemPrompt =
            BuildSystemPrompt(
                call.AiAgent.Instructions,
                previousMessages,
                knowledgeResults);

        // -------------------------------------------------
        // AI cevabını üret
        // -------------------------------------------------

        var answer =
            await _aiChatService.AskAsync(
                new AiChatRequest
                {
                    Provider =
                        call.AiAgent.Provider,

                    ModelName =
                        call.AiAgent.ModelName,

                    SystemPrompt =
                        systemPrompt,

                    UserMessage =
                        customerText
                },
                cancellationToken);

        if (string.IsNullOrWhiteSpace(answer))
        {
            answer =
                "Şu anda bu soruya yanıt veremiyorum.";
        }

        // -------------------------------------------------
        // AI cevabını CallMessages'a kaydet
        // -------------------------------------------------

        var aiMessage =
            await CreateAiMessageAsync(
                request.CompanyId,
                request.CallId,
                answer,
                cancellationToken);

        return new ProcessCallCustomerMessageResponse
        {
            CallId = call.Id,

            CustomerMessage =
                ToDto(customerMessage),

            AiMessage =
                ToDto(aiMessage),

            Answer =
                answer,

            HandledByHuman = false
        };
    }

    private async Task<
        ProcessCallCustomerMessageResponse>
        ProcessHumanTransferAsync(
            ProcessCallCustomerMessageCommand request,
            Call call,
            CallMessage customerMessage,
            IReadOnlyList<CallMessage> previousMessages,
            IReadOnlyList<string> knowledgeResults,
            AiTransferDecision transferDecision,
            CancellationToken cancellationToken)
    {
        // -------------------------------------------------
        // Güvenlik kontrolü
        // -------------------------------------------------

        if (!call.AiAgent.TransferToHumanEnabled)
        {
            const string disabledAnswer =
                "Şu anda insan temsilciye aktarım yapılamıyor. " +
                "Size buradan yardımcı olmaya devam edebilirim.";

            var disabledMessage =
                await CreateAiMessageAsync(
                    request.CompanyId,
                    request.CallId,
                    disabledAnswer,
                    cancellationToken);

            return new ProcessCallCustomerMessageResponse
            {
                CallId = call.Id,

                CustomerMessage =
                    ToDto(customerMessage),

                AiMessage =
                    ToDto(disabledMessage),

                Answer =
                    disabledAnswer,

                HandledByHuman = false
            };
        }

        // -------------------------------------------------
        // Aktarım kuyruğu tanımlı mı?
        // -------------------------------------------------

        if (!call.AiAgent.TransferCallQueueId.HasValue)
        {
            const string noQueueAnswer =
                "İnsan temsilci aktarımı açık ancak " +
                "aktarım kuyruğu henüz yapılandırılmamış.";

            var noQueueMessage =
                await CreateAiMessageAsync(
                    request.CompanyId,
                    request.CallId,
                    noQueueAnswer,
                    cancellationToken);

            return new ProcessCallCustomerMessageResponse
            {
                CallId = call.Id,

                CustomerMessage =
                    ToDto(customerMessage),

                AiMessage =
                    ToDto(noQueueMessage),

                Answer =
                    noQueueAnswer,

                HandledByHuman = false
            };
        }

        // -------------------------------------------------
        // AI devir özeti oluştur
        // -------------------------------------------------

        var conversationHistory =
            BuildHandoffConversationHistory(
                previousMessages,
                customerMessage);

        var handoffSummaryResult =
            await _handoffSummaryService.CreateAsync(
                new AiHandoffSummaryRequest
                {
                    CustomerMessage =
                        customerMessage.Content,

                    TransferReason =
                        transferDecision.Reason ??
                        "İnsan operatör desteği gerekli.",

                    ConversationHistory =
                        conversationHistory,

                    KnowledgeResults =
                        knowledgeResults
                },
                cancellationToken);

        var aiHandoffSummary =
            string.IsNullOrWhiteSpace(
                handoffSummaryResult.Summary)
                ? null
                : handoffSummaryResult.Summary.Trim();

        // -------------------------------------------------
        // Müşteriye aktarım bilgisini ver
        //
        // Bu AI'nın müşteriye verdiği SON mesajdır.
        // Bundan sonraki müşteri mesajlarında
        // call.TransferredToHuman = true olacağı için
        // AI artık cevap üretmeyecektir.
        // -------------------------------------------------

        const string transferAnswer =
            "Elbette. Sizi uygun bir müşteri " +
            "temsilcisine aktarıyorum.";

        var aiMessage =
            await CreateAiMessageAsync(
                request.CompanyId,
                request.CallId,
                transferAnswer,
                cancellationToken);

        // -------------------------------------------------
        // Call durumunu değiştireceğimiz için
        // tracked entity yükle
        // -------------------------------------------------

        var trackedCall =
            await _callRepository
                .GetTrackedByIdAsync(
                    request.CompanyId,
                    request.CallId,
                    cancellationToken);

        if (trackedCall is null)
        {
            throw new KeyNotFoundException(
                "Aktarılacak çağrı bulunamadı.");
        }

        // -------------------------------------------------
        // Çağrıyı insan aktarımı durumuna getir
        // -------------------------------------------------

        trackedCall.TransferToHumanQueue();

        // -------------------------------------------------
        // Mevcut CallQueue altyapısına gönder
        //
        // TransferReason + AiHandoffSummary birlikte
        // kuyruk kaydına aktarılır.
        // -------------------------------------------------

        await _sender.Send(
            new EnqueueCallCommand(
                request.CompanyId,
                call.AiAgent
                    .TransferCallQueueId
                    .Value,
                request.CallId,
                null,
                transferDecision.Reason,
                aiHandoffSummary),
            cancellationToken);

        // -------------------------------------------------
        // Call değişikliğini kaydet
        // -------------------------------------------------

        await _callRepository.SaveChangesAsync(
            cancellationToken);

        return new ProcessCallCustomerMessageResponse
        {
            CallId = call.Id,

            CustomerMessage =
                ToDto(customerMessage),

            AiMessage =
                ToDto(aiMessage),

            Answer =
                transferAnswer,

            HandledByHuman = true
        };
    }

    private async Task<CallMessage>
        CreateAiMessageAsync(
            Guid companyId,
            Guid callId,
            string answer,
            CancellationToken cancellationToken)
    {
        var aiSequenceNumber =
            await _callMessageRepository
                .GetNextSequenceNumberAsync(
                    companyId,
                    callId,
                    cancellationToken);

        var aiMessage =
            new CallMessage(
                callId,
                MessageSpeaker.AiAgent,
                answer,
                aiSequenceNumber);

        await _callMessageRepository.AddAsync(
            aiMessage,
            cancellationToken);

        await _callMessageRepository.SaveChangesAsync(
            cancellationToken);

        await NotifyMessageCreatedAsync(
            companyId,
            aiMessage,
            cancellationToken);

        return aiMessage;
    }

    private static IReadOnlyList<string>
        BuildHandoffConversationHistory(
            IReadOnlyList<CallMessage> previousMessages,
            CallMessage currentCustomerMessage)
    {
        var history =
            previousMessages
                .OrderBy(
                    x => x.SequenceNumber)
                .Select(
                    x =>
                        $"{GetSpeakerName(x.Speaker)}: {x.Content}")
                .ToList();

        history.Add(
            $"{GetSpeakerName(currentCustomerMessage.Speaker)}: " +
            currentCustomerMessage.Content);

        return history;
    }

    private static string GetSpeakerName(
        MessageSpeaker speaker)
    {
        return speaker switch
        {
            MessageSpeaker.Customer =>
                "Müşteri",

            MessageSpeaker.AiAgent =>
                "Yapay Zekâ",

            MessageSpeaker.HumanAgent =>
                "Operatör",

            MessageSpeaker.System =>
                "Sistem",

            _ =>
                "Bilinmiyor"
        };
    }

    private static string BuildKnowledgeSearchQuery(
        IReadOnlyList<CallMessage> previousMessages,
        string currentMessage)
    {
        var recentCustomerMessages =
            previousMessages
                .Where(
                    x =>
                        x.Speaker ==
                        MessageSpeaker.Customer)
                .OrderBy(
                    x => x.SequenceNumber)
                .TakeLast(3)
                .Select(
                    x => x.Content);

        return string.Join(
            " ",
            recentCustomerMessages.Append(
                currentMessage));
    }

    private static string BuildSystemPrompt(
        string agentInstructions,
        IReadOnlyList<CallMessage> previousMessages,
        IReadOnlyList<string> knowledgeResults)
    {
        var prompt =
            new StringBuilder();

        if (!string.IsNullOrWhiteSpace(
                agentInstructions))
        {
            prompt.AppendLine(
                agentInstructions.Trim());

            prompt.AppendLine();
        }

        prompt.AppendLine(
            "Bu bir telefon çağrısıdır.");

        prompt.AppendLine(
            "Müşteriye kısa, doğal ve anlaşılır cevaplar ver.");

        prompt.AppendLine(
            "Bilgi bankasını öncelikli kaynak olarak kullan.");

        prompt.AppendLine(
            "Bilgi bulunamazsa bunu açıkça belirt ve bilgi uydurma.");

        // -------------------------------------------------
        // Önceki konuşma geçmişi
        // -------------------------------------------------

        if (previousMessages.Count > 0)
        {
            prompt.AppendLine();

            prompt.AppendLine(
                "ÖNCEKİ TELEFON KONUŞMASI:");

            foreach (
                var message in
                previousMessages
                    .OrderBy(
                        x => x.SequenceNumber))
            {
                var speaker =
                    message.Speaker switch
                    {
                        MessageSpeaker.Customer =>
                            "Müşteri",

                        MessageSpeaker.AiAgent =>
                            "Asistan",

                        MessageSpeaker.HumanAgent =>
                            "Temsilci",

                        _ =>
                            "Sistem"
                    };

                prompt.AppendLine(
                    $"{speaker}: {message.Content}");
            }
        }

        // -------------------------------------------------
        // Knowledge Base sonuçları
        // -------------------------------------------------

        if (knowledgeResults.Count > 0)
        {
            prompt.AppendLine();

            prompt.AppendLine(
                "BİLGİ BANKASI İÇERİĞİ:");

            for (
                var index = 0;
                index < knowledgeResults.Count;
                index++)
            {
                prompt.AppendLine();

                prompt.AppendLine(
                    $"Kaynak {index + 1}:");

                prompt.AppendLine(
                    knowledgeResults[index]);
            }
        }
        else
        {
            prompt.AppendLine();

            prompt.AppendLine(
                "Bu soru için bilgi bankasında ilgili kayıt bulunamadı.");
        }

        return prompt.ToString();
    }

    private static CallMessageDto ToDto(
        CallMessage message)
    {
        return new CallMessageDto(
            message.Id,
            message.CallId,
            message.Speaker,
            message.Content,
            message.SpokenAt,
            message.SequenceNumber,
            message.ConfidenceScore,
            message.AudioFileUrl);
    }

    private async Task NotifyMessageCreatedAsync(
        Guid companyId,
        CallMessage message,
        CancellationToken cancellationToken)
    {
        await _realtimeNotifier
            .MessageCreatedAsync(
                new CallRealtimeMessage(
                    message.Id,
                    companyId,
                    message.CallId,
                    message.Speaker,
                    message.Content,
                    message.SpokenAt,
                    message.SequenceNumber,
                    message.ConfidenceScore,
                    message.AudioFileUrl),
                cancellationToken);
    }
}