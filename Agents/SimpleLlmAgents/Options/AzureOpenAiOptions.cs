namespace SimpleLlmAgents.Options
{
    public class AzureOpenAiOptions
    {
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }
        public List<string> Deployments { get; set; }
    }
}
