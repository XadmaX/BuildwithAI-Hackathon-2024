using Assistant.Service.Models;

namespace Assistant.Service.Interfaces;

public interface IConversationService
{
    Task<Conversation> CreateAsync(ConversationCreateData data, CancellationToken cancellationToken = default);
    Task<ConversationResponse> SendAsync(ConversationText conversation, CancellationToken cancellationToken = default);
}