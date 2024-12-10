namespace AssistantManage.Data.Models;

public class ConversationEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public List<string> Documents { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public List<MessageEntity> Messages { get; set; } = new();

    public string UserId { get; set; }

    public string ThreadId { get; set; }

    public Guid AssistantId { get; set; }
    public AssistantEntity Assistant { get; set; } = new();
}