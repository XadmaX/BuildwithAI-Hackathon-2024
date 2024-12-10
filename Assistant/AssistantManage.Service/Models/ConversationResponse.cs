namespace Assistant.Service.Models;

public class ConversationResponse : Conversation
{
    public List<string> Attachments { get; set; } = [];
    public List<TextAnnotation> Annotations { get; set; } = [];
    public ModelMessageResponse InputMessage { get; set; }
    public ModelMessageResponse OutputMessage { get; set; }
}