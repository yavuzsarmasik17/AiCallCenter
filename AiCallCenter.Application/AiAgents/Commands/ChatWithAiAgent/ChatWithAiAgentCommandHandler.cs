using System.Text;
using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Application.AiConversations.Interfaces;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Application.Common.Knowledge;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.ChatWithAiAgent;

public sealed class ChatWithAiAgentCommandHandler
    : IRequestHandler<
        ChatWithAiAgentCommand,
        ChatWithAiAgentResponse>
{
    private readonly IAiAgentRepository _aiAgentRepository;
    private readonly IAiConversationRepository _conversationRepository;
    private readonly IAiChatService _aiChatService;
    private readonly IKnowledgeSearchService _knowledgeSearchService;

    public ChatWithAiAgentCommandHandler(
        IAiAgentRepository aiAgentRepository,
        IAiConversationRepository conversationRepository,
        IAiChatService aiChatService,
        IKnowledgeSearchService knowledgeSearchService)
    {
        _aiAgentRepository = aiAgentRepository;
        _conversationRepository = conversationRepository;
        _aiChatService = aiChatService;
        _knowledgeSearchService = knowledgeSearchService;
    }

    public async Task<ChatWithAiAgentResponse> Handle(
        ChatWithAiAgentCommand request,
        CancellationToken cancellationToken)
    {
        var aiAgent = await _aiAgentRepository.GetByIdAsync(
            request.CompanyId,
            request.AiAgentId,
            cancellationToken);

        if (aiAgent is null)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }

        if (!aiAgent.IsActive)
        {
            throw new InvalidOperationException(
                "Yapay zekâ ajanı aktif değil.");
        }

        AiConversation conversation;

        if (request.ConversationId.HasValue)
        {
            conversation =
                await _conversationRepository.GetActiveByIdAsync(
                    request.CompanyId,
                    request.AiAgentId,
                    request.ConversationId.Value,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    "Aktif sohbet kaydı bulunamadı.");
        }
        else
        {
            conversation = new AiConversation(
                request.CompanyId,
                request.AiAgentId);

            await _conversationRepository.AddConversationAsync(
                conversation,
                cancellationToken);

            await _conversationRepository.SaveChangesAsync(
                cancellationToken);
        }

        var previousMessages =
            await _conversationRepository.GetMessagesAsync(
                conversation.Id,
                maximumMessageCount: 20,
                cancellationToken);

        var knowledgeSearchQuery = BuildKnowledgeSearchQuery(
            previousMessages,
            request.Message);

        var knowledgeResults =
            await _knowledgeSearchService.SearchAsync(
                request.CompanyId,
                request.AiAgentId,
                knowledgeSearchQuery,
                maximumResultCount: 5,
                cancellationToken);

        var systemPrompt = BuildSystemPrompt(
            aiAgent.Instructions,
            previousMessages,
            knowledgeResults);

        var nextSequenceNumber =
            await _conversationRepository
                .GetNextSequenceNumberAsync(
                    conversation.Id,
                    cancellationToken);

        var customerMessage = new AiConversationMessage(
            conversation.Id,
            MessageSpeaker.Customer,
            request.Message,
            nextSequenceNumber);

        await _conversationRepository.AddMessageAsync(
            customerMessage,
            cancellationToken);

        var answer = await _aiChatService.AskAsync(
            new AiChatRequest
            {
                Provider = aiAgent.Provider,
                ModelName = aiAgent.ModelName,
                SystemPrompt = systemPrompt,
                UserMessage = request.Message
            },
            cancellationToken);

        var aiMessage = new AiConversationMessage(
            conversation.Id,
            MessageSpeaker.AiAgent,
            answer,
            nextSequenceNumber + 1);

        await _conversationRepository.AddMessageAsync(
            aiMessage,
            cancellationToken);

        await _conversationRepository.SaveChangesAsync(
            cancellationToken);

        return new ChatWithAiAgentResponse
        {
            ConversationId = conversation.Id,
            AiAgentId = aiAgent.Id,
            AiAgentName = aiAgent.Name,
            UserMessage = request.Message,
            Answer = answer
        };
    }

    private static string BuildKnowledgeSearchQuery(
        IReadOnlyList<AiConversationMessage> previousMessages,
        string currentMessage)
    {
        var recentCustomerMessages = previousMessages
            .Where(message =>
                message.Speaker == MessageSpeaker.Customer)
            .TakeLast(3)
            .Select(message => message.Content);

        return string.Join(
            " ",
            recentCustomerMessages.Append(currentMessage));
    }

    private static string BuildSystemPrompt(
        string agentInstructions,
        IReadOnlyList<AiConversationMessage> previousMessages,
        IReadOnlyList<string> knowledgeResults)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine(agentInstructions);
        prompt.AppendLine();

        prompt.AppendLine(
            "Bilgi bankasını öncelikli kaynak olarak kullan.");

        prompt.AppendLine(
            "Bilgi bulunamazsa bunu açıkça belirt ve bilgi uydurma.");

        if (previousMessages.Count > 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("ÖNCEKİ KONUŞMA:");

            foreach (var message in previousMessages)
            {
                var speaker = message.Speaker switch
                {
                    MessageSpeaker.Customer => "Müşteri",
                    MessageSpeaker.AiAgent => "Asistan",
                    MessageSpeaker.HumanAgent => "Temsilci",
                    _ => "Sistem"
                };

                prompt.AppendLine(
                    $"{speaker}: {message.Content}");
            }
        }

        if (knowledgeResults.Count > 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("BİLGİ BANKASI İÇERİĞİ:");

            for (var index = 0;
                 index < knowledgeResults.Count;
                 index++)
            {
                prompt.AppendLine();
                prompt.AppendLine($"Kaynak {index + 1}:");
                prompt.AppendLine(knowledgeResults[index]);
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
}