using Assistant.Service.Exceptions;
using Assistant.Service.Interfaces;
using Assistant.Service.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Assistants;
using System.Collections.Concurrent;
using System.Text;
using TextAnnotation = Assistant.Service.Models.TextAnnotation;

namespace Assistant.Service.Services;

internal class ConversationService : IConversationService
{
    private readonly AssistantClient _client;
    private readonly IFunctionService _functionService;
    private readonly IDistributedLock _distributedLock;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        AssistantClient client,
        IFunctionService functionService,
        IDistributedLock distributedLock,
        ILogger<ConversationService> logger)
    {
        _client = client;
        _functionService = functionService;
        _distributedLock = distributedLock;
        _logger = logger;
    }

    public async Task<Conversation> CreateAsync(ConversationCreateData data,
        CancellationToken cancellationToken = default)
    {
        AssistantThread thread = await _client.CreateThreadAsync(new ThreadCreationOptions()
        {
        }, cancellationToken);

        return new Conversation()
        {
            ThreadId = thread.Id,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="conversation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FailedAcquireLockException"></exception>
    public async Task<ConversationResponse> SendAsync(ConversationText conversation,
        CancellationToken cancellationToken = default)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var conversationResponse = new ConversationResponse();

        var locker = await _distributedLock.AcquireLockAsync(conversation.ThreadId);
        if (string.IsNullOrWhiteSpace(locker))
        {
            _logger.LogInformation("Failed to acquire lock");
            throw new FailedAcquireLockException($"Failed to acquire lock for thread {conversation.ThreadId}");
        }
        {
            try
            {
                ThreadMessage inputMessage = await _client.CreateMessageAsync(
                    conversation.ThreadId,
                    MessageRole.User,
                    [conversation.Text],
                    new MessageCreationOptions(),
                    source.Token);

                var textResponseBuilder = new StringBuilder();

                var response = await HandleStreaming(
                    _client.CreateRunStreamingAsync(
                        conversation.ThreadId,
                        conversation.AssistantId,
                        new RunCreationOptions(),
                        source.Token),
                    textResponseBuilder,
                    new HandleRunModel(),
                    source.Token);

                conversationResponse.Attachments = response.AttachmentFiles ?? [];
                conversationResponse.Annotations = response.Annotations ?? [];

                ThreadRun run = _client.GetRun(conversation.ThreadId, response.RunId);

                conversationResponse.InputMessage = new ModelMessageResponse()
                {
                    MessageId = inputMessage.Id,
                    Text = conversation.Text,
                    TokenCount = run.Usage.InputTokenCount
                };
                conversationResponse.OutputMessage = new ModelMessageResponse()
                {
                    MessageId = response.MessageId,
                    Text = textResponseBuilder.ToString(),
                    TokenCount = run.Usage.OutputTokenCount
                };

                return conversationResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await source.CancelAsync();
                var runs = _client.GetRuns(conversation.ThreadId).Where(w => w.Status != RunStatus.Cancelled || w.Status != RunStatus.Cancelling || w.Status != RunStatus.Completed || w.Status != RunStatus.Expired).OrderByDescending(o => o.CreatedAt).Take(15);

                foreach (var run in runs)
                {
                    try
                    {
                        _client.CancelRun(conversation.ThreadId, run.Id, cancellationToken);
                    }
                    catch (Exception exp)
                    {
                        _logger.LogError(exp, exp.Message);
                    }
                }
                throw;
            }
            finally
            {
                await _distributedLock.ReleaseLockAsync(conversation.ThreadId, locker);
                Console.WriteLine($"{DateTime.Now} Thread {conversation.ThreadId} is unlocked by message {conversation.Text}");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>ResponseMessageId, RunId and Annotations</returns>
    private async Task<HandleRunModel> HandleStreaming(IAsyncEnumerable<StreamingUpdate> streaming,
        StringBuilder messageBuilder, HandleRunModel runModel,
        CancellationToken cancellationToken = default)
    {
        await foreach (var update in streaming.WithCancellation(cancellationToken))
        {
            switch (update)
            {
                case RequiredActionUpdate requiredAction:
                    ConcurrentBag<ToolOutput> outputs = [];
                    await Parallel.ForEachAsync(requiredAction.Value.RequiredActions, cancellationToken, async (action, token) =>
                    {
                        outputs.Add(await _functionService.ExecuteFunctionAsync(action, cancellationToken));
                    });

                    runModel = await HandleStreaming(
                        _client.SubmitToolOutputsToRunStreamingAsync(
                            requiredAction.Value.ThreadId, requiredAction.Value.Id,
                            outputs, cancellationToken), messageBuilder,
                        runModel, cancellationToken);
                    return runModel;             // імперично визначено що далі йти не треба
                    //break;                     // імперично визначено що далі йти не треба
                case MessageContentUpdate contentUpdate:
                    messageBuilder.Append(contentUpdate.Text);
                    runModel.MessageId = contentUpdate.MessageId;

                    if (contentUpdate.TextAnnotation != null)
                        runModel.Annotations.Add(TextAnnotation.FromTextAnnotation(contentUpdate.TextAnnotation));

                    if (contentUpdate.ImageFileId != null)
                        runModel.AttachmentFiles.Add(contentUpdate.ImageFileId);

                    break;
                case RunUpdate run:
                    if (run.UpdateKind
                        is StreamingUpdateReason.Error
                        or StreamingUpdateReason.MessageFailed
                        or StreamingUpdateReason.RunCancelled
                        or StreamingUpdateReason.RunFailed)
                    {
                        Console.WriteLine();
                    }

                    runModel.RunId = run.Value.Id;
                    break;
                default:
                    break;
            }
        }

        if (runModel.Annotations.Any())
            runModel.Annotations = runModel.Annotations.DistinctBy(d => d.TextToReplace).ToList();

        return runModel;
    }
}