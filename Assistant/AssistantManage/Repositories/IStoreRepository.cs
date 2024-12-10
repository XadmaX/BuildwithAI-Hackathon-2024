using AssistantManage.Data.Models;

namespace AssistantManage.Repositories;

public interface IStoreRepository
{
    public Task<AssistantEntity?> AddAssistantAsync(AssistantEntity assistantEntity,
        CancellationToken cancellationToken = default);
    public IQueryable<AssistantEntity> GetAssistants();
    public AssistantEntity? GetAssistantById(Guid id);

    public Task<VectorStoreEntity?> AddStoreAsync(VectorStoreEntity vectorStore,
        CancellationToken cancellationToken = default);

    public Task<MessageEntity?> AddMessageAsync(MessageEntity messageEntity,
        CancellationToken cancellationToken = default);
    public IQueryable<MessageEntity> GetMessages(Guid conversationId, int? pageIndex = 0, int? pageSize = 100);

    public Task<ConversationEntity?> AddConversationAsync(ConversationEntity conversationEntity,
        CancellationToken cancellationToken = default);
    public IQueryable<ConversationEntity> GetConversationsForUser(string userId);
    public ConversationEntity? GetConversationById(Guid id);

    public Task<VectoredFileEntity?> GetFileData(string inModelFileId, CancellationToken cancellationToken = default);
}