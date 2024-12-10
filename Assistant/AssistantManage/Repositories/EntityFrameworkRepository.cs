using AssistantManage.Data;
using AssistantManage.Data.Enums;
using AssistantManage.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantManage.Repositories;

public class EntityFrameworkRepository : IStoreRepository
{
    private readonly AssistantDbContext _context;
    private readonly ILogger<EntityFrameworkRepository> _logger;

    public EntityFrameworkRepository(AssistantDbContext context, ILogger<EntityFrameworkRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IQueryable<AssistantEntity> GetAssistants()
    {
        return _context.Assistants
            .AsNoTracking()
            .Where(w => w.Status == Status.Enabled);
    }

    /// <inheritdoc />
    public AssistantEntity? GetAssistantById(Guid id)
    {
        return _context.Assistants.Find(id);
    }

    public async Task<AssistantEntity?> AddAssistantAsync(AssistantEntity assistantEntity,
        CancellationToken cancellationToken = default)
    {
        return await AddAsync(assistantEntity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorStoreEntity?> AddStoreAsync(VectorStoreEntity vectorStore,
        CancellationToken cancellationToken = default)
    {
        return await AddAsync(vectorStore, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MessageEntity?> AddMessageAsync(MessageEntity messageEntity,
        CancellationToken cancellationToken = default)
    {
        return await AddAsync(messageEntity, cancellationToken);
    }

    /// <inheritdoc />
    public IQueryable<MessageEntity> GetMessages(Guid conversationId, int? pageIndex, int? pageSize)
    {
        pageIndex ??= 0;
        pageSize ??= 100;

        return _context.Messages
            .AsNoTracking()
            .Where(w => w.ConversationId == conversationId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(pageIndex.Value * pageSize.Value)
            .Take(pageSize.Value);
    }

    /// <inheritdoc />
    public async Task<ConversationEntity?> AddConversationAsync(ConversationEntity conversationEntity,
        CancellationToken cancellationToken = default)
    {
        return await AddAsync(conversationEntity, cancellationToken);
    }

    /// <inheritdoc />
    public IQueryable<ConversationEntity> GetConversationsForUser(string userId)
    {
        return _context.Conversations
            .AsNoTracking()
            .Where(w => w.UserId == userId);
    }

    /// <inheritdoc />
    public ConversationEntity? GetConversationById(Guid id)
    {
        var sss = _context.Conversations.ToList();

        return _context.Conversations
            .Include(i => i.Assistant)
            .FirstOrDefault(f => f.Id == id);
    }

    /// <inheritdoc />
    public async Task<VectoredFileEntity?> GetFileData(string inModelFileId, CancellationToken cancellationToken = default)
    {
        return await _context.Files.FirstOrDefaultAsync(f => f.InModelId == inModelFileId,
            cancellationToken: cancellationToken);
    }

    private async Task<T?> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        T? entityResult = null;
        try
        {
            entityResult = (await _context.Set<T>().AddAsync(entity, cancellationToken)).Entity;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return entityResult;
    }
}