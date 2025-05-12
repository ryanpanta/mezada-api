using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Models
{
    public class TaskHistoryModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TaskId { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public string AccountedBy { get; set; } = string.Empty;
        public string ChildId { get; set; } = string.Empty;
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsReverted { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
