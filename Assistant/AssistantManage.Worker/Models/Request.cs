namespace AssistantManage.Worker.Models
{
    public class Request
    {
        public string AssistantId { get; set; }
        public string ThreadId { get; set; }
        public string Prompt { get; set; }  
        public object ConversationReference { get; set; }

        public string ToString()
        {
            return $"{AssistantId}|{ThreadId}";
        }
    }
}
