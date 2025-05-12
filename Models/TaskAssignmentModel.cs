using MongoDB.Bson.Serialization.Attributes;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Models
{
    public class TaskAssignmentModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }
        public string TaskId { get; set; } = string.Empty;
        public string ChildId { get; set; } = string.Empty;
        public int CustomIncrement { get; set; }
        public int CurrentBalance { get; set; } = 0;
        public int BonusBalance { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;

    }
}
