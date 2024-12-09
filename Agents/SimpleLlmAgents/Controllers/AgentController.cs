using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using SimpleLlmAgents.Services.SmpleLlmAgent;
using SimpleLlmAgents.Services.SmpleLlmAgent.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace SimpleLlmAgents.Controllers
{
    [Route("agent")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly SimpleLlmAgentService _simpleLlmAgentService;

        public AgentController(SimpleLlmAgentService simpleLlmAgentService)
        {
            _simpleLlmAgentService = simpleLlmAgentService;
        }


        [HttpGet]
        [SwaggerOperation(Summary = "Get a list of agents", Description = "Returns a list of all available agents.", OperationId = "GetAgents")]
        [SwaggerResponse(StatusCodes.Status200OK, "List of agents", typeof(List<Agent>))]
        public async Task<IActionResult> Get()
        {
            return Ok(await _simpleLlmAgentService.GetAgentsAsync());
        }

        [HttpPost("{name}")]
        [SwaggerOperation(Summary = "Call an agent by name", Description = "Calls an agent with the specified name and request parameters.", OperationId = "CallAgent")]
        [SwaggerResponse(StatusCodes.Status200OK, "Response from the agent", typeof(CallAgentResponse))]
        public async Task<IActionResult> Post(string name, [FromBody] KernelArguments request)
        {
            return Ok(await _simpleLlmAgentService.CallAgentAsync(name, request));
        }
    }
}
