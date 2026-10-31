using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class CallQueueItemRepository : ICallQueueItemRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CallQueueItemRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CallQueue?> GetQueueAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueues
            .FirstOrDefaultAsync(
                x => x.CompanyId == companyId &&
                     x.Id == callQueueId,
                cancellationToken);
    }

    public Task<Call?> GetCallAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Calls
            .FirstOrDefaultAsync(
                x => x.CompanyId == companyId &&
                     x.Id == callId,
                cancellationToken);
    }

    public Task<CallQueueItem?> GetByIdAsync(
        Guid companyId,
        Guid queueItemId,
        CancellationToken cancellationToken = default)
    {
        // ÖNEMLİ:
        // Burada AsNoTracking kullanılmıyor.
        //
        // Çünkü bu metot ile getirilen CallQueueItem üzerinde
        // Accept(), Complete(), Cancel(), Expire() gibi
        // değişiklikler yapılabiliyor.
        //
        // EF Core'un bu entity'yi takip etmesi gerekiyor ki
        // SaveChangesAsync çağrıldığında değişiklikler
        // veritabanına UPDATE olarak gönderilsin.

        return _dbContext.CallQueueItems
            .Include(x => x.Call)
            .Include(x => x.CallQueue)
            .Include(x => x.AssignedOperator)
            .FirstOrDefaultAsync(
                x => x.CompanyId == companyId &&
                     x.Id == queueItemId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CallQueueItem>> GetAllAsync(
        Guid companyId,
        CallQueueStatus? status = null,
        Guid? callQueueId = null,
        CancellationToken cancellationToken = default)
    {
        var query = CreateListQuery(companyId);

        if (status.HasValue)
        {
            query = query.Where(
                x => x.Status == status.Value);
        }

        if (callQueueId.HasValue)
        {
            query = query.Where(
                x => x.CallQueueId == callQueueId.Value);
        }

        return await query
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.WaitingSince)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CallQueueItem>> GetWaitingAsync(
        Guid companyId,
        Guid? callQueueId = null,
        CancellationToken cancellationToken = default)
    {
        var query = CreateListQuery(companyId)
            .Where(
                x => x.Status == CallQueueStatus.Waiting);

        if (callQueueId.HasValue)
        {
            query = query.Where(
                x => x.CallQueueId == callQueueId.Value);
        }

        return await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.WaitingSince)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CallQueueItem>> GetByOperatorAsync(
        Guid companyId,
        Guid operatorId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = CreateListQuery(companyId)
            .Where(
                x => x.AssignedOperatorId == operatorId);

        if (activeOnly)
        {
            query = query.Where(
                x => x.Status == CallQueueStatus.Assigned ||
                     x.Status == CallQueueStatus.Accepted);
        }

        return await query
            .OrderByDescending(x => x.AssignedAt)
            .ThenByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ActiveItemExistsAsync(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueItems
            .AnyAsync(
                x => x.CompanyId == companyId &&
                     x.CallId == callId &&
                     (
                         x.Status == CallQueueStatus.Waiting ||
                         x.Status == CallQueueStatus.Assigned ||
                         x.Status == CallQueueStatus.Accepted
                     ),
                cancellationToken);
    }

    public Task<int> GetActiveQueueSizeAsync(
        Guid companyId,
        Guid callQueueId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.CallQueueItems
            .CountAsync(
                x => x.CompanyId == companyId &&
                     x.CallQueueId == callQueueId &&
                     (
                         x.Status == CallQueueStatus.Waiting ||
                         x.Status == CallQueueStatus.Assigned ||
                         x.Status == CallQueueStatus.Accepted
                     ),
                cancellationToken);
    }

    public async Task AddAsync(
        CallQueueItem queueItem,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.CallQueueItems
            .AddAsync(
                queueItem,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private IQueryable<CallQueueItem> CreateListQuery(
        Guid companyId)
    {
        // Listeleme işlemlerinde entity üzerinde değişiklik
        // yapılmadığı için AsNoTracking burada kalabilir.
        // Bu kullanım okuma performansı açısından daha uygundur.

        return _dbContext.CallQueueItems
            .AsNoTracking()
            .Include(x => x.Call)
            .Include(x => x.CallQueue)
            .Include(x => x.AssignedOperator)
            .Where(x => x.CompanyId == companyId);
    }
}