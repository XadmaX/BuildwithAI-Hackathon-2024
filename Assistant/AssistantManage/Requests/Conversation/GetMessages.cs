using AssistantManage.Models;
using AssistantManage.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssistantManage.Requests.Conversation;

public class GetMessages : IRequest<List<Message>>
{
    public Guid ConversationId { get; }
    public int? PageIndex { get; }
    public int? PageSize { get; }

    public GetMessages(Guid conversationId, int? pageIndex, int? pageSize)
    {
        ConversationId = conversationId;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}

public class GetMessagesHandler : IRequestHandler<GetMessages, List<Message>>
{
    private readonly IStoreRepository _repository;

    public GetMessagesHandler(IStoreRepository repository)
    {
        _repository = repository;
    }
    
    /// <inheritdoc />
    public async Task<List<Message>> Handle(GetMessages request, CancellationToken cancellationToken)
    {
        return await _repository.GetMessages(request.ConversationId, request.PageIndex, request.PageSize)
            .Select(s => new Message(s.Text, s.Sender.ToString()))
            .ToListAsync(cancellationToken);
    }
}