using System.Collections.Concurrent;
using Assistant.Service.Builder;
using Microsoft.AspNetCore.Http;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;

namespace Assistant.Service.Extensions;

internal static class AssistantExtensions
{
    public static AssistantBuilder AssistantBuilder(this AssistantClient client) => new(client);

    public static FileSearchToolResources CreateFileSearchResource(this OpenAIFileClient client,
        IEnumerable<IFormFile>? files, out List<OpenAIFile> outFiles)
    {
        var openAiFiles = new ConcurrentBag<OpenAIFile>();

        if (files is not null && !(files ?? []).Any())
        {
            Parallel.ForEach(files,
                file =>
                {
                    openAiFiles.Add(client.UploadFile(file.OpenReadStream(), file.FileName,
                        FileUploadPurpose.Assistants));
                });
        }

        var vectorStoreCreation = new VectorStoreCreationHelper(openAiFiles)
        {
            ChunkingStrategy = FileChunkingStrategy.Auto
        };

        outFiles = openAiFiles.ToList();
        return new FileSearchToolResources
        {
            NewVectorStores = { vectorStoreCreation }
        };
    }
}