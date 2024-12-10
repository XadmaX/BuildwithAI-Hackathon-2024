using Assistant.Service.Interfaces;
using MediatR;

namespace AssistantManage.Requests.Assistant;

public class DeleteAll : IRequest
{
    
}

public class DeleteAllHandler : IRequestHandler<DeleteAll>
{
    private readonly IAssistantService _assistantService;

    public DeleteAllHandler(IAssistantService assistantService)
    {
        _assistantService = assistantService;
    }
    
    /// <inheritdoc />
    public async Task Handle(DeleteAll request, CancellationToken cancellationToken)
    {
        await _assistantService.DeleteAllAsync(cancellationToken);
    }
}
