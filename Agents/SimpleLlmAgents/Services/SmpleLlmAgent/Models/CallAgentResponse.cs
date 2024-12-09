using Microsoft.SemanticKernel;

namespace SimpleLlmAgents.Services.SmpleLlmAgent.Models
{
    public class CallAgentResponse
    {
        public CallAgentResponse(FunctionResult functionResult)
        {
            Message = functionResult.ToString();
            Usage = functionResult.Metadata?.SingleOrDefault(m => m.Key == "Usage").Value;
        }
        public string Message { get; set; }
        public object? Usage { get; set; }

    }
}
