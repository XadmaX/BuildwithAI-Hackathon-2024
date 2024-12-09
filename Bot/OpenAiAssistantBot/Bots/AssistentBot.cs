using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatGPTBot.Models;
using ChatGPTBot.Options.Azure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ca.Ai.Assistant;
using MongoDB.Driver.Core.Operations;
using System.Net.Mime;
using System.Runtime.Intrinsics.X86;

namespace ChatGPTBot.Bots
{
    public class AssistentBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly BlobContainerClient _blobClient;
        private readonly ILogger<AssistentBot> _logger;
        private readonly IAssistantService _assistantService;
        private readonly IConfiguration _configuration;

        public AssistentBot(
            ConversationState conversationState,
            BlobContainerClient blobClient,
            IAssistantService assistantService,
            IConfiguration configuration,
            ILogger<AssistentBot> logger)
        {
            _conversationState = conversationState;
            _blobClient = blobClient;
            _logger = logger;
            _assistantService = assistantService;
            _configuration = configuration;
        }
        protected async override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationProfile = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData(), cancellationToken);
            if (conversationProfile.AssistantConversationId != Guid.Empty)
            {
                return;
            }
            var assisantId = _configuration.GetValue<Guid>("AssistantService:AssistantId");

            if (assisantId != Guid.Empty)
            {
                await ConnectToAsisstantAsync(turnContext, assisantId, cancellationToken);
                await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
            }
        }
        protected async override Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationProfile = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData(), cancellationToken);
            if (conversationProfile.AssistantConversationId != Guid.Empty)
            {
                return;
            }
            var eventName = turnContext.Activity.Name;
            if (eventName == "StartNewConversation")
            {
                try
                {
                    // Отримайте параметри з поля Value
                    var parameters = turnContext.Activity.Value;

                    if (parameters is JObject paramObj)
                    {
                        var assistantId = paramObj["assistantId"].ToString();

                        if (string.IsNullOrEmpty(assistantId))
                        {
                            throw new Exception("Сталася помилка при запуску асистента. Перевантажте сторінку.");
                        }

                        if (Guid.TryParse(assistantId, out var assistantIdGuid))
                        {
                            await ConnectToAsisstantAsync(turnContext, assistantIdGuid, cancellationToken);
                        }
                        else
                        {
                            throw new Exception($"Сталася помилка при запуску асистента. Перевантажте сторінку. AssistantId: {assistantId}");

                        }
                    }
                    else
                    {
                        throw new Exception("Сталася помилка при запуску асистента. Перевантажте сторінку.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    await turnContext.SendActivityAsync(ex.Message);
                    await turnContext.SendActivityAsync(ex.StackTrace);
                }
            }
            else
            {
                await base.OnEventActivityAsync(turnContext, cancellationToken);
            }

        }

        private async Task ConnectToAsisstantAsync(ITurnContext turnContext, Guid assistantIdGuid, CancellationToken cancellationToken)
        {
            var userId = turnContext.Activity.From.Id;
            var assistantConversationId = await _assistantService.CreateConversationAsync(assistantIdGuid, userId, cancellationToken);
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationProfile = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData(), cancellationToken);
            conversationProfile.AssistantConversationId = Guid.Parse(assistantConversationId);
            conversationProfile.Id = turnContext.Activity.Conversation.Id;

            await _conversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);

            await turnContext.SendActivityAsync(Activity.CreateTypingActivity(), cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(Activity.CreateTypingActivity(), cancellationToken);
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationProfile = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData(), cancellationToken);
            if (conversationProfile.AssistantConversationId == Guid.Empty)
            {
                var assisantId = _configuration.GetValue<Guid>("AssistantService:AssistantId");
                if (assisantId == Guid.Empty)
                {
                    return;
                }
                await ConnectToAsisstantAsync(turnContext, assisantId, cancellationToken);
            }

            var conversationReference = turnContext.Activity.GetConversationReference();

            if (turnContext.Activity.Attachments != null && turnContext.Activity.Attachments.Any())
            {
                try
                {
                    await _assistantService.SendMessageAsync(
                        conversationProfile.AssistantConversationId,
                        $"Системне повідомлення: Поясни користувачу що наразі немає можливості обробити вкладення",
                        true,
                        "bot-service",
                        conversationReference,
                        cancellationToken);
                }
                catch (ApiException ex)
                {
                    if (ex.StatusCode != StatusCodes.Status202Accepted)
                        throw;
                }
            }

            if (string.IsNullOrEmpty(turnContext.Activity.Text))
                return;

            try
            {
                await _assistantService.SendMessageAsync(
                    conversationProfile.AssistantConversationId,
                    turnContext.Activity.Text,
                    true,
                    "bot-service",
                    conversationReference,
                    cancellationToken);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode != StatusCodes.Status202Accepted)
                    throw;
            }

        }
    }
}
