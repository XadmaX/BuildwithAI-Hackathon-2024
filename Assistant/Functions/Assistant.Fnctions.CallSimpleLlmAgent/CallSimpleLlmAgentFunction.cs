using Assistant.Agents.SimpleLlmAgent;
using Assistant.Functions.Abstractions.Interfaces;
using System.ComponentModel;

namespace CallSimpleLlmAgent
{
    [Description("Call a stateless agent")]
    public class CallSimpleLlmAgentFunction : IFunction<CallSimpleLlmAgentRequest>
    {
        private readonly SimpleLlmAgentService _simpleLlmAgentService;

        public CallSimpleLlmAgentFunction(SimpleLlmAgentService simpleLlmAgentService)
        {
            _simpleLlmAgentService = simpleLlmAgentService;
        }
        public async Task<object> ExecuteFunction(CallSimpleLlmAgentRequest request, CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, object>
            {
                { "prompt", request.Prompt }
            };
            var response = await _simpleLlmAgentService.CallAgentAsync(request.Name, body);
            return response.Message;
        }
    }
}
