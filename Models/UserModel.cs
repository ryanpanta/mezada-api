using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Reflection.Metadata.Ecma335;
using WebApiMezada.Models.Enums;

namespace WebApiMezada.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public EnumRoles Role { get; set; } = EnumRoles.Child;
        public bool Active { get; set; } = true;
        public string FamilyGroupId { get; set; } = string.Empty;
        public ICollection<string> Tasks { get; set; } = new List<string>();

        public void SetParent()
        {
            Role = EnumRoles.Parent;
        }

    }
}
