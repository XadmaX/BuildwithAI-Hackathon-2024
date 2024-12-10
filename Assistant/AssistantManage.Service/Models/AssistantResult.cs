using OpenAI.Files;
using OpenAI.VectorStores;

namespace Assistant.Service.Models;

public class AssistantResult
{
    public OpenAI.Assistants.Assistant Assistant { get; set; }
    public VectorStore? Store { get; set; }
    public List<OpenAIFile>? AIFiles { get; set; }
}