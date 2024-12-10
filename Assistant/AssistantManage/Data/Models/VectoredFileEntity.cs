using System.ComponentModel.DataAnnotations;

namespace AssistantManage.Data.Models
{
    public class VectoredFileEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public string Filename { get; set; }
        public string Purpose { get; set; }
        public int SizeInBytes { get; set; }
        //public string BlobPath { get; set; }
        
        // sources
        public string? ExternalSource { get; set; } 

        public string InModelId { get; set; }
    }
}