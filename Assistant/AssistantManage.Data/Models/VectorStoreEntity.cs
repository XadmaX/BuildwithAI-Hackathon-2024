namespace AssistantManage.Data.Models;

public class VectorStoreEntity : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public string InModelId { get; set; }

    public List<VectoredFileEntity> Files { get; set; } = new();
}