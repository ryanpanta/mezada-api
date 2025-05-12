using MongoDB.Bson.Serialization.Attributes;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Models
{
    public class TaskModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string CycleId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public EnumCategory Category { get; set; }
        public string FamilyGroupId { get; set; }
        public string UserId { get; set; }
        public int InitialValue { get; set; } = 0;
        public int DefaultIncrement { get; set; }
        public int LimitValue { get; set; }
        public bool Active { get; set; } = true;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    }
}
