using Assistant.Service.Models;
using AssistantManage.Models;
using AssistantManage.Requests.Conversation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using Conversation = AssistantManage.Models.Conversation;

namespace AssistantManage.Controllers;

[ApiController]
[Route("conversation")]
public class ConversationController : ControllerBase
{
    private readonly ISender _sender;

    public ConversationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{conversationId:guid}/{text}")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ConversationResponse),
        ContentTypes = [MediaTypeNames.Application.Json])]
    [SwaggerResponse(StatusCodes.Status202Accepted, Type = typeof(void),
        ContentTypes = [MediaTypeNames.Application.Json])]
    [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(string), ContentTypes = [MediaTypeNames.Text.Plain])]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(string),
        ContentTypes = [MediaTypeNames.Text.Plain])]
    [SwaggerOperation("Send a message to a conversation", OperationId = "SendMessage")]
    public async Task<IActionResult> SendAsync([FromRoute] Guid conversationId, string text, bool isAsync = false,
        string? source = null, object conversationReference = null,
        CancellationToken cancellationToken = default)
    {
        if (isAsync)
        {
            await _sender.Send(new SendMessage(conversationId, text, isAsync, source, conversationReference),
                cancellationToken);
            return Accepted();
        }
        else
        {
            return Ok(await _sender.Send(new SendMessage(conversationId, text), cancellationToken));
        }
    }

    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(string), ContentTypes = [MediaTypeNames.Text.Plain])]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(string),
        ContentTypes = [MediaTypeNames.Text.Plain])]
    [SwaggerOperation("Create a new conversation", OperationId = "CreateConversation")]
    public async Task<IActionResult> CreateConversation(Guid assistantId, string userId,
        CancellationToken cancellationToken)
    {
        return Ok((await _sender.Send(new CreateConversation(assistantId, userId),
            cancellationToken)).ToString());
    }

    [HttpGet("{conversationId}/history")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<Message>),
        ContentTypes = [MediaTypeNames.Application.Json])]
    [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(string), ContentTypes = [MediaTypeNames.Text.Plain])]
    [SwaggerOperation("Get the message history of a conversation", OperationId = "GetHistory")]
    public async Task<IActionResult> GetHistoryAsync(Guid conversationId, int? pageIndex, int pageSize,
        CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetMessages(conversationId, pageIndex, pageSize), cancellationToken));
    }

    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<Conversation>),
        ContentTypes = [MediaTypeNames.Application.Json])]
    [SwaggerResponse(StatusCodes.Status404NotFound, Type = typeof(string), ContentTypes = [MediaTypeNames.Text.Plain])]
    [SwaggerOperation("Get all conversations for a user", OperationId = "GetConversations")]
    public async Task<IActionResult> GetConversationsAsync(string userId, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetConversations(userId), cancellationToken));
    }
}