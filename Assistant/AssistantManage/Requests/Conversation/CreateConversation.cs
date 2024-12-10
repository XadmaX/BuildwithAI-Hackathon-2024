using Assistant.Service.Interfaces;
using Assistant.Service.Models;
using AssistantManage.Data.Models;
using AssistantManage.Repositories;
using MediatR;

namespace AssistantManage.Requests.Conversation;

public class CreateConversation : IRequest<Guid>
{
    public Guid AssistantId { get; }
    public string UserId { get; }

    public CreateConversation(Guid assistantId, string userId)
    {
        AssistantId = assistantId;
        UserId = userId;
    }
}

public class CreateConversationHandler : IRequestHandler<CreateConversation, Guid>
{
    private readonly IConversationService _conversationService;
    private readonly IStoreRepository _repository;

    public CreateConversationHandler(IConversationService conversationService, IStoreRepository repository)
    {
        _conversationService = conversationService;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<Guid> Handle(CreateConversation data, CancellationToken cancellationToken)
    {
        var assistant = _repository.GetAssistantById(data.AssistantId);

        var thread = await _conversationService.CreateAsync(new ConversationCreateData()
        {
        }, cancellationToken);

        var conversation = await _repository.AddConversationAsync(new ConversationEntity()
        {
            ThreadId = thread.ThreadId,
            UserId = data.UserId,
            AssistantId = data.AssistantId,
            Assistant = assistant,
        }, cancellationToken);

        return conversation.Id;
    }
}