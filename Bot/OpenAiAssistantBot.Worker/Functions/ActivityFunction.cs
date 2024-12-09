using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAiAssistantBot.Worker.Models;

namespace OpenAiAssistantBot.Worker.Functions
{
    public class ActivityFunction
    {
        private readonly ILogger<ActivityFunction> _logger;
        private readonly CloudAdapter _adapter;
        private readonly IConfiguration _configuration;

        public ActivityFunction(
            ILogger<ActivityFunction> logger,
            CloudAdapter adapter,
            IConfiguration configuration)
        {
            _logger = logger;
            _adapter = adapter;
            _configuration = configuration; 
        }

        [Function(nameof(ReturnActivity))]
        public async Task ReturnActivity(
            [ServiceBusTrigger(
                "outgoing", 
                "bot-service",
                Connection = "ServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            var messageBody = message.Body.ToString();
            var request = new Request();
            try
            {
                request = JsonConvert.DeserializeObject<Request>(messageBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize message body");
                await messageActions.CompleteMessageAsync(message);
                return;
            }
            if (request.ConversationReference.ChannelId == null)
            {
                _logger.LogError(
                    new ArgumentNullException(nameof(request.ConversationReference)),
                    "ConversationReference is required");
                await messageActions.CompleteMessageAsync(message);
                return;
            }
            if (string.IsNullOrEmpty(request.Prompt))
            {
                _logger.LogError(
                    new ArgumentNullException(nameof(request.Prompt)),
                    "Prompt is required");
                await messageActions.CompleteMessageAsync(message);
                return;
            }

            var activity = MessageFactory.Text(request.Prompt);
            activity.TextFormat = TextFormatTypes.Plain;

            await _adapter.ContinueConversationAsync(
                    botAppId: _configuration["MicrosoftAppId"] ?? string.Empty,
                    reference: request.ConversationReference,
                    callback: async (turnContext, cancellationToken) =>
                    {
                        await turnContext.SendActivityAsync(activity, cancellationToken: cancellationToken);
                    },
                    cancellationToken: default);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
