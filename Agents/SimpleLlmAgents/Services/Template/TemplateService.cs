using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using SimpleLlmAgents.Options;
using Microsoft.SemanticKernel;
using System.Reflection.Metadata;

namespace SimpleLlmAgents.Services.Template
{
    public class TemplateService
    {
        private readonly BlobServiceClient _blobStorageClient;
        private readonly BlobContainerClient _containerClient;
        private readonly Kernel _kernel;

        public TemplateService(
            IOptions<AzureStorageOptions> azureStorageOption,
            BlobContainerClient blobContainerClient,
            Kernel kernel)
        {
            _containerClient = blobContainerClient;
            _kernel = kernel;
        }
        public async Task<List<Models.Template>> GetTmplateListAsync()
        {
            var result = new List<Models.Template>();
            var blobs = _containerClient.GetBlobsAsync();
            await foreach (var blob in _containerClient.GetBlobsAsync())
            {
                var extension = Path.GetExtension(blob.Name);
                if (extension == ".prompty")
                {
                    // parse the file and get the name
                    var blobClient = _containerClient.GetBlobClient(blob.Name);
                    var response = await blobClient.DownloadAsync();
                    var content = await new StreamReader(response.Value.Content).ReadToEndAsync();
#pragma warning disable SKEXP0040
                    var func = _kernel.CreateFunctionFromPrompty(content);
#pragma warning restore SKEXP0040
                    result.Add(new Models.Template(
                        func.Name, 
                        func.Description, 
                        func.Metadata.Parameters.Select(p => new Models.Parameter(p.Name, p.Description, p.IsRequired))
                            .ToList(),
                        blob.Name
                        ));
                }
            }
            return result;
        }
        public async Task<string> GetTemplateAsync(string name)
        {
            var blobClient = _containerClient.GetBlobClient($"{name}.prompty");
            var response = await blobClient.DownloadAsync();
            var content = await new StreamReader(response.Value.Content).ReadToEndAsync();
            return content;
        }
    }
}
