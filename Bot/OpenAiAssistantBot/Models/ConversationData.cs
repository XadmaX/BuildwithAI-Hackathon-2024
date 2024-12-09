namespace ChatGPTBot.Models
{
    public class ConversationData
    {
        public string Id { get; set; }
        public List<string> Documents { get; set; } = new List<string>();
        public Dictionary<DateTime, KeyValuePair<string, string>> Messages { get; set; } = new Dictionary<DateTime, KeyValuePair<string, string>>();
        public Guid AssistantConversationId { get; set; }
    }
}
