using Assistant.Service.Exceptions;
using Assistant.Service.Interfaces;
using Assistant.Service.Models;
using AssistantManage.Worker.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AssistantManage.Worker.Functions
{
    public class ProcessFunction
    {
        private readonly ILogger _logger;
        private readonly IConversationService _conversationService;
        private readonly ServiceBusSender _serviceBusSender;

        public ProcessFunction(
            ILoggerFactory loggerFactory,
            IConversationService conversationService,
            ServiceBusClient serviceBusClient)
        {
            _logger = loggerFactory.CreateLogger<ProcessFunction>();
            _conversationService = conversationService;
            _serviceBusSender = serviceBusClient.CreateSender("outgoing");
        }

        [Function(nameof(ProcessFunction))]
        public async Task Run(
            [ServiceBusTrigger("incoming", Connection = "ServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var request = JsonConvert.DeserializeObject<Request>(message.Body.ToString());
            var source = message.ApplicationProperties["source"] as string;
            var threadId = request.ThreadId;
            var expiry = TimeSpan.FromSeconds(60);

            try
            {
                _logger.LogInformation($"{DateTime.Now} Thread {threadId} is locked by message {request.Prompt}");

                var response = await _conversationService.SendAsync(new ConversationText()
                {
                    Text = request.Prompt,
                    ThreadId = threadId,
                    AssistantId = request.AssistantId
                });

                #region call after execution

                var resultMessage = new
                {
                    Prompt = response.OutputMessage.Text,
                    ConversationReference = request.ConversationReference
                };
                var resultServiceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(resultMessage));
                resultServiceBusMessage.ApplicationProperties.Add("source", source);
                await _serviceBusSender.SendMessageAsync(resultServiceBusMessage);

                #endregion

                await messageActions.CompleteMessageAsync(message);
            }
            catch (FailedAcquireLockException ex)
            {
                _logger.LogError(ex, ex.Message);
                await messageActions.AbandonMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await messageActions.DeadLetterMessageAsync(message);
            }
        }
    }
}