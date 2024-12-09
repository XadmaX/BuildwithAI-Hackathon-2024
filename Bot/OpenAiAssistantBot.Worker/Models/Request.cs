using Microsoft.Bot.Schema;

namespace OpenAiAssistantBot.Worker.Models
{
    public class Request
    {
        public ConversationReference ConversationReference { get; set; }
        public string Prompt { get; set; }
    }
}
