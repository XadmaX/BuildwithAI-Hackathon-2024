using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CallSimpleLlmAgent
{
    public class CallSimpleLlmAgentRequest
    {
        [Description("The system name of the agent being called.")]
        [Required]
        public string Name { get; set; }

        [Description("The prompt or message to send to the agent.")]
        [Required]
        public string Prompt { get; set; }

        [Description("A list of previous chat history items, if any, for context.")]
        [Required]
        public List<ChatHistoryItem> ChatHistory { get; set; }
    }

    public class ChatHistoryItem
    {
        [Required]
        [Description("The question asked in the chat history item.")]
        public string Question { get; set; }

        [Required]
        [Description("The answer provided in the chat history item.")]
        public string Answer { get; set; }
    }

}
