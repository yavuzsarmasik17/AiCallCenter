using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class AiAgent : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Instructions { get; private set; } = string.Empty;

    public string WelcomeMessage { get; private set; } = string.Empty;

    public AgentLanguage Language { get; private set; }

    public string VoiceName { get; private set; } = "alloy";

    public int MaxCallDurationMinutes { get; private set; }

    public bool TransferToHumanEnabled { get; private set; }

    // İnsan temsilciye aktarım yapılacağı zaman
    // kullanılacak varsayılan çağrı kuyruğu.
    public Guid? TransferCallQueueId { get; private set; }

    public bool IsActive { get; private set; }

    public AiProvider Provider { get; private set; }
        = AiProvider.OpenAI;

    public string ModelName { get; private set; }
        = "gpt-5-mini";

    public Company Company { get; private set; } = null!;

    public ICollection<Call> Calls { get; private set; }
        = new List<Call>();

    public ICollection<AiConversation> Conversations
    { get; private set; } = new List<AiConversation>();

    public ICollection<AiAgentKnowledgeBase> KnowledgeBases
    { get; private set; } = new List<AiAgentKnowledgeBase>();

    public ICollection<CallOperatorAiAgent> OperatorAssignments
    { get; private set; } = new List<CallOperatorAiAgent>();

    private AiAgent()
    {
    }

    public AiAgent(
        Guid companyId,
        string name,
        string instructions,
        string welcomeMessage,
        AgentLanguage language = AgentLanguage.Turkish,
        AiProvider provider = AiProvider.OpenAI,
        string modelName = "gpt-5-mini")
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        CompanyId = companyId;

        SetName(name);
        SetInstructions(instructions);
        SetWelcomeMessage(welcomeMessage);
        SetProvider(provider, modelName);

        Language = language;
        VoiceName = "alloy";
        MaxCallDurationMinutes = 15;
        TransferToHumanEnabled = true;
        TransferCallQueueId = null;
        IsActive = true;
    }

    public void UpdatePrompt(
        string instructions,
        string welcomeMessage)
    {
        SetInstructions(instructions);
        SetWelcomeMessage(welcomeMessage);
    }

    public void ChangeVoice(string voiceName)
    {
        if (string.IsNullOrWhiteSpace(voiceName))
        {
            throw new ArgumentException(
                "Ses adı boş olamaz.",
                nameof(voiceName));
        }

        VoiceName = voiceName.Trim();
    }

    public void ChangeLanguage(AgentLanguage language)
    {
        Language = language;
    }

    public void ChangeMaxCallDuration(int minutes)
    {
        if (minutes < 1 || minutes > 120)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minutes),
                "Çağrı süresi 1 ile 120 dakika arasında olmalıdır.");
        }

        MaxCallDurationMinutes = minutes;
    }

    public void EnableHumanTransfer()
    {
        TransferToHumanEnabled = true;
    }

    public void DisableHumanTransfer()
    {
        TransferToHumanEnabled = false;
    }

    public void SetTransferCallQueue(
        Guid? callQueueId)
    {
        if (callQueueId.HasValue &&
            callQueueId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Aktarım kuyruğu kimliği geçersiz.",
                nameof(callQueueId));
        }

        TransferCallQueueId = callQueueId;
    }

    public void ClearTransferCallQueue()
    {
        TransferCallQueueId = null;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Update(
        string name,
        string instructions,
        string welcomeMessage,
        AgentLanguage language)
    {
        SetName(name);
        SetInstructions(instructions);
        SetWelcomeMessage(welcomeMessage);
        Language = language;
    }

    public void Rename(string name)
    {
        SetName(name);
    }

    public void SetProvider(
        AiProvider provider,
        string modelName)
    {
        if (!Enum.IsDefined(provider))
        {
            throw new ArgumentException(
                "Yapay zekâ sağlayıcısı geçersiz.",
                nameof(provider));
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException(
                "Model adı boş olamaz.",
                nameof(modelName));
        }

        if (modelName.Trim().Length > 200)
        {
            throw new ArgumentException(
                "Model adı en fazla 200 karakter olabilir.",
                nameof(modelName));
        }

        Provider = provider;
        ModelName = modelName.Trim();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Yapay zekâ ajanı adı boş olamaz.",
                nameof(name));
        }

        if (name.Trim().Length > 150)
        {
            throw new ArgumentException(
                "Yapay zekâ ajanı adı en fazla 150 karakter olabilir.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private void SetInstructions(
        string instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            throw new ArgumentException(
                "Ajan talimatları boş olamaz.",
                nameof(instructions));
        }

        if (instructions.Trim().Length > 20000)
        {
            throw new ArgumentException(
                "Ajan talimatları en fazla 20000 karakter olabilir.",
                nameof(instructions));
        }

        Instructions = instructions.Trim();
    }

    private void SetWelcomeMessage(
        string welcomeMessage)
    {
        if (string.IsNullOrWhiteSpace(welcomeMessage))
        {
            throw new ArgumentException(
                "Karşılama mesajı boş olamaz.",
                nameof(welcomeMessage));
        }

        if (welcomeMessage.Trim().Length > 1000)
        {
            throw new ArgumentException(
                "Karşılama mesajı en fazla 1000 karakter olabilir.",
                nameof(welcomeMessage));
        }

        WelcomeMessage = welcomeMessage.Trim();
    }
}