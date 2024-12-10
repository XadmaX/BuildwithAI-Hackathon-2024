using Assistant.Functions.Abstractions.Interfaces;
using Azure.AI.OpenAI;
using OpenAI.Images;

namespace ImageGenerator
{
    public class ImageGeneratorFunction : IFunction<ImageGeneratorRequest>
    {
        private readonly ImageClient _dalleImageClient;

        public ImageGeneratorFunction(AzureOpenAIClient azureOpenAIClient)
        {
            _dalleImageClient = azureOpenAIClient.GetImageClient("dall-e-3");
        }
        public async Task<object> ExecuteFunction(ImageGeneratorRequest request, CancellationToken cancellationToken = default)
        {
            switch(request.ModelType)
            {
                case ModelType.DALL_E:
                    try
                    {
                        var imageResult = await _dalleImageClient.GenerateImageAsync(request.Prompt);
                        return imageResult.Value.ImageUri;
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
