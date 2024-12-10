using System.Text;
using Assistant.Service.Exceptions;
using Assistant.Service.Interfaces;
using Assistant.Service.Models;
using AssistantManage.Data.Enums;
using AssistantManage.Data.Models;
using AssistantManage.Repositories;
using Azure.Messaging.ServiceBus;
using MediatR;
using Newtonsoft.Json;

namespace AssistantManage.Requests.Conversation;

public class SendMessage : IRequest<ConversationResponse>
{
    public Guid ConversationId { get; }
    public string Message { get; }
    public bool IsAsync { get; }
    public string? Source { get; }
    public object ConversationReference { get; }

    public SendMessage(Guid conversationId, string message, bool isAsync = false, string? source = null,
        object? conversationReference = null)
    {
        ConversationId = conversationId;
        Message = message;
        IsAsync = isAsync;
        Source = source;
        ConversationReference = conversationReference;
    }
}

public class SendMessageHandler : IRequestHandler<SendMessage, ConversationResponse>
{
    private readonly IConversationService _conversationService;
    private readonly IStoreRepository _repository;
    private readonly ServiceBusSender _serviceBusSender;

    public SendMessageHandler(IConversationService conversationService, IStoreRepository repository,
        ServiceBusClient serviceBusClient)
    {
        _conversationService = conversationService;
        _repository = repository;
        _serviceBusSender = serviceBusClient.CreateSender("incoming");
    }

    /// <inheritdoc />
    public async Task<ConversationResponse> Handle(SendMessage request, CancellationToken cancellationToken)
    {
        var conversationResponse = new ConversationResponse();

        var conversation = _repository.GetConversationById(request.ConversationId);
        ArgumentNullException.ThrowIfNull(conversation);

        if (request.IsAsync)
        {
            var message = new
            {
                AssistantId = conversation.Assistant.InModelId,
                ThreadId = conversation.ThreadId,
                Prompt = request.Message,
                ConversationReference = request.ConversationReference,
            };
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            serviceBusMessage.ApplicationProperties.Add("source", request.Source);
            await _serviceBusSender.SendMessageAsync(serviceBusMessage, cancellationToken);
            return default;
        }

        {
            var response = await _conversationService.SendAsync(new ConversationText()
            {
                Text = request.Message,
                ThreadId = conversation.ThreadId,
            }, cancellationToken);

            await _repository.AddMessageAsync(new MessageEntity()
            {
                Text = request.Message,
                Sender = Sender.User,
                ConversationId = conversation.Id,
                AssistantMessageId = response.InputMessage.MessageId,
                TokenUsage = response.InputMessage.TokenCount
            }, cancellationToken);

            await _repository.AddMessageAsync(new MessageEntity()
            {
                Text = response.OutputMessage.Text,
                ConversationId = conversation.Id,
                AssistantMessageId = response.OutputMessage.MessageId,
                Sender = Sender.Assistant,
                TokenUsage = response.OutputMessage.TokenCount,
                Annotations = response.Annotations.Select(s => new Annotation()
                {
                    EndIndex = s.EndIndex,
                    StartIndex = s.StartIndex,
                    InputFileId = s.InputFileId,
                    OutputFileId = s.OutputFileId,
                    TextToReplace = s.TextToReplace,
                }).ToList()
            }, cancellationToken);
        }

        return conversationResponse;
    }
}