using AssistantManage.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssistantManage.Requests.Conversation;

public class GetConversations : IRequest<List<Models.Conversation>>
{
    public string UserId { get; }

    public GetConversations(string userId)
    {
        UserId = userId;
    }
}

public class GetConversationsHandler : IRequestHandler<GetConversations, List<Models.Conversation>>
{
    private readonly IStoreRepository _repository;

    public GetConversationsHandler(IStoreRepository repository)
    {
        _repository = repository;
    }
    
    /// <inheritdoc />
    public async Task<List<Models.Conversation>> Handle(GetConversations request, CancellationToken cancellationToken)
    {
        return await _repository.GetConversationsForUser(request.UserId)
            .Select(s => new Models.Conversation(s.Id, s.Messages.Count))
            .ToListAsync(cancellationToken: cancellationToken);
    }
}