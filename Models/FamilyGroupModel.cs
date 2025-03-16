using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Models
{
    public class FamilyGroupModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HashCode { get; set; } = string.Empty;
        public ICollection<string> Users { get; set; } = new List<string>();
        public bool Active { get; set; } = true;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public ICollection<string> Tasks { get; set; } = new List<string>();

    }
}
