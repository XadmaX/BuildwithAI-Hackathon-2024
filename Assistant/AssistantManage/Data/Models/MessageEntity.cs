using AssistantManage.Data.Enums;
using OpenAI.Assistants;

namespace AssistantManage.Data.Models;

public class MessageEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public string Text { get; set; }
    public Sender Sender { get; set; }

    public int TokenUsage { get; set; }

    public List<Annotation> Annotations { get; set; } = new List<Annotation>();

    public string AssistantMessageId { get; set; }

    public Guid ConversationId { get; set; }
}

public class Annotation
{
    public int? StartIndex { get; set; }
    public int? EndIndex { get; set; }
    
    public string TextToReplace { get; set; }
    
    public string InputFileId   { get; set; }
    public string OutputFileId { get; set; }
}