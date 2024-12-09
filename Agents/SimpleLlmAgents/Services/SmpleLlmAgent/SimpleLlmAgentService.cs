using Azure.Storage.Blobs;
using Microsoft.SemanticKernel;
using SimpleLlmAgents.Services.SmpleLlmAgent.Models;
using SimpleLlmAgents.Services.Template;

namespace SimpleLlmAgents.Services.SmpleLlmAgent
{
    public class SimpleLlmAgentService
    {
        private BlobServiceClient _blobServiceClient;
        private readonly TemplateService _templateService;
        private readonly Kernel _kernel;

        public SimpleLlmAgentService(
            TemplateService templateService,
            Kernel kernel)
        {
            _templateService = templateService;
            _kernel = kernel;
        }
        public async Task<List<Agent>> GetAgentsAsync()
        {
            var templates = await _templateService.GetTmplateListAsync();
            var agents = templates.Select(t => new Agent(t)).ToList();
            return agents;
        }
        public async Task<CallAgentResponse> CallAgentAsync(string name, KernelArguments request)
        {
            var template = await _templateService.GetTemplateAsync(name);
#pragma warning disable SKEXP0040
            var func = _kernel.CreateFunctionFromPrompty(template);
#pragma warning restore SKEXP0040
            var result = await func.InvokeAsync(_kernel, arguments: request);
            return new CallAgentResponse(result);
        }
    }
}
