using Assistant.Functions.Abstractions.Interfaces;
using AssistantManage.Data.Enums;

namespace AssistantManage.Data.Models
{
    public class AssistantEntity : BaseEntity
    {
        public string InModelId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Base64Avatar { get; set; }

        public Status Status { get; set; }

        public float Temperature { get; set; }

        public bool HasFileSearch { get; set; }
        public bool HasCodeInterpreter { get; set; }

        public AssistantTools Tools { get; set; }

        public Guid? StoreId { get; set; }
        public VectorStoreEntity? Store { get; set; }
    }
}