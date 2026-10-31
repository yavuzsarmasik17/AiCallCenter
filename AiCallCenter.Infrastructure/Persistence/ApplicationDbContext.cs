using AiCallCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies
        => Set<Company>();

    public DbSet<AiAgent> AiAgents
        => Set<AiAgent>();

    public DbSet<Call> Calls
        => Set<Call>();

    public DbSet<CallMessage> CallMessages
        => Set<CallMessage>();

    public DbSet<AiConversation> AiConversations
        => Set<AiConversation>();

    public DbSet<AiConversationMessage> AiConversationMessages
        => Set<AiConversationMessage>();

    public DbSet<KnowledgeBase> KnowledgeBases
        => Set<KnowledgeBase>();

    public DbSet<KnowledgeDocument> KnowledgeDocuments
        => Set<KnowledgeDocument>();

    public DbSet<KnowledgeChunk> KnowledgeChunks
        => Set<KnowledgeChunk>();

    public DbSet<AiAgentKnowledgeBase> AiAgentKnowledgeBases
        => Set<AiAgentKnowledgeBase>();

    public DbSet<CallQueue> CallQueues
    => Set<CallQueue>();

    public DbSet<CallQueueOperator> CallQueueOperators
        => Set<CallQueueOperator>();

    public DbSet<User> Users
        => Set<User>();

    public DbSet<CallOperator> CallOperators
        => Set<CallOperator>();

    public DbSet<CallOperatorAiAgent> CallOperatorAiAgents
        => Set<CallOperatorAiAgent>();

    public DbSet<CallQueueItem> CallQueueItems
        => Set<CallQueueItem>();

    public DbSet<CallEvent> CallEvents
        => Set<CallEvent>();

    public DbSet<CallNote> CallNotes
        => Set<CallNote>();

    public DbSet<CallRecording> CallRecordings
        => Set<CallRecording>();

    public DbSet<PasswordResetCode> PasswordResetCodes
        => Set<PasswordResetCode>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}