using Assistant.Functions.Abstractions.Interfaces;
using Assistant.Service.Extensions;
using Assistant.Service.Interfaces;
using Assistant.Service.Models;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;

namespace Assistant.Service.Services;

internal class AssistantService : IAssistantService
{
    private readonly AssistantClient _assistantClient;
    private readonly OpenAIFileClient _fileClient;
    private readonly VectorStoreClient _vectorStoreClient;

    public AssistantService(AssistantClient assistantClient, OpenAIFileClient fileClient,
        VectorStoreClient vectorStoreClient)
    {
        _assistantClient = assistantClient;
        _fileClient = fileClient;
        _vectorStoreClient = vectorStoreClient;
    }

    public async Task<AssistantResult> AddAssistantAsync(
        AssistantCreateData data,
        CancellationToken cancellationToken = default)
    {
        var result = new AssistantResult();

        var builder = _assistantClient.AssistantBuilder()
            .WithName(data.Name)
            .WithDescription(data.Description)
            .WithInstructions(data.Instructions)
            .WithTemperature(data.Temperature);

        foreach (var function in data.Functions)
        {
            if (function is AssistantTools.None)
                continue;

            builder.AddTool(function);
        }

        if (data.CodeInterpreter)
        {
            builder.WithCodeInterpreter();
        }

        if (data.FileSearch)
        {
            var resource = _fileClient.CreateFileSearchResource(data.Files, out var aiFiles);

            result.AIFiles = aiFiles;

            builder.WithFileSearch();
            builder.WithFileSearchResource(resource);
        }

        result.Assistant = await builder.CreateAsync(cancellationToken);

        if (result.Assistant.ToolResources is { FileSearch: not null })
        {
            result.Store =
                _vectorStoreClient.GetVectorStore(result.Assistant.ToolResources.FileSearch.VectorStoreIds.FirstOrDefault());
        }

        return result;
    }

    public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        var assistants = _assistantClient.GetAssistants().ToArray();
        foreach (var assistant in assistants)
        {
            await _assistantClient.DeleteAssistantAsync(assistant.Id, cancellationToken);
        }

        var files = (await _fileClient.GetFilesAsync(cancellationToken)).Value
            .ToArray();
        foreach (var file in files)
        {
            await _fileClient.DeleteFileAsync(file.Id, cancellationToken);
        }

        var stores = _vectorStoreClient.GetVectorStores().ToArray();
        foreach (var store in stores)
        {
            await _vectorStoreClient.DeleteVectorStoreAsync(store.Id, cancellationToken);
        }
    }
}