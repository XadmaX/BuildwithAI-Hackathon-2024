using Newtonsoft.Json;
using System.ComponentModel;

namespace ImageGenerator
{
    public class ImageGeneratorRequest
    {
        [Description("Specifies the type of AI model to use for image generation.")]
        public ModelType ModelType { get; set; }

        [Description("The text prompt describing the desired image to be generated.")]
        public string Prompt { get; set; }
    }

    public enum ModelType
    {
        [Description("DALL·E model for image generation.")]
        DALL_E,
    }
}