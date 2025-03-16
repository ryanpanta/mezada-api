using MongoDB.Bson.Serialization.Attributes;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Models
{
    public class TaskModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public EnumTaskStatus Status { get; set; } = EnumTaskStatus.Pending;
        public string FamilyGroupId { get; set; }
        public string UserId { get; set; }
        public int Points { get; set; }

    }
}
