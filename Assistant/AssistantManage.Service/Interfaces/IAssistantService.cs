using Assistant.Service.Models;

namespace Assistant.Service.Interfaces;

public interface IAssistantService
{
    Task<AssistantResult> AddAssistantAsync(AssistantCreateData createData, CancellationToken cancellationToken = default);

    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}