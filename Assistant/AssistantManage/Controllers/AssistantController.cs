using AssistantManage.Data;
using AssistantManage.Requests.Assistant;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AssistantManage.Controllers;

[ApiController]
[Route("assistant")]
public class AssistantController : ControllerBase
{
    private readonly ISender _sender;
    private readonly AssistantDbContext _context;

    public AssistantController(ISender sender, AssistantDbContext context)
    {
        _sender = sender;
        _context = context;
    }

    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, "Assistant Id", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(string))]
    [SwaggerOperation("Create a new assistant", OperationId = "CreateAssistant")]
    public async Task<IActionResult> Create([FromForm] [Required] string name, [FromForm] string? description,
        [FromForm] [Required] string instructions, IFormFileCollection? files,
        [FromForm] [Range(0.01f, 1f)] float temperature = 1f, [FromForm] bool fileSearch = false,
        [FromForm] bool codeInterpreter = false, CancellationToken cancellationToken = default,
        [FromForm] params Assistant.Functions.Abstractions.Interfaces.AssistantTools[] functions)
    {
        if (fileSearch && (files == null || !files.Any()))
            return BadRequest("For FileSearch, the files must be specified");
        if ((files == null || files.Any()) && !fileSearch)
            return BadRequest("For uploading files, the FileSearch flag must be specified");

        return Ok(await _sender.Send(new CreateAssistant(name, description, instructions, files, functions, temperature,
            fileSearch, codeInterpreter), cancellationToken));
    }

    [HttpDelete]
    [SwaggerResponse(StatusCodes.Status200OK, "All assistants, files, and vector stores are deleted", typeof(void))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(string))]
    [SwaggerOperation("Delete all assistants, files, and vector stores", OperationId = "DeleteAll")]
    public async Task<IActionResult> DeleteAllAsync(CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAll(), cancellationToken);

        await _context.Assistants.ExecuteDeleteAsync(cancellationToken);
        return Ok();
    }

    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "List of assistants", typeof(IEnumerable<Models.Assistant>))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(string))]
    [SwaggerOperation("Get all assistants", OperationId = "GetAssistants")]
    public async Task<IActionResult> GetAssistants(CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetAllAssistants(), cancellationToken));
    }
}