using OpenAI.Assistants;

namespace Assistant.Service.Interfaces;

public interface IFunctionService
{
    Task<ToolOutput> ExecuteFunctionAsync(RequiredAction action, CancellationToken cancellationToken = default);
}