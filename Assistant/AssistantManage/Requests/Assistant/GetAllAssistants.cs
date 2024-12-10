using AssistantManage.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssistantManage.Requests.Assistant;

public class GetAllAssistants : IRequest<List<Models.Assistant>>
{
}

public class GetAllAssistantsHandler : IRequestHandler<GetAllAssistants, List<Models.Assistant>>
{
    private readonly IStoreRepository _repository;

    public GetAllAssistantsHandler(IStoreRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<List<Models.Assistant>> Handle(GetAllAssistants request, CancellationToken cancellationToken)
    {
        return await _repository.GetAssistants().Select(s => new Models.Assistant(s.Id, s.Name, s.Base64Avatar))
            .ToListAsync(cancellationToken: cancellationToken);
    }
}