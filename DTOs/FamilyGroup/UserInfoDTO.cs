using WebApiMezada.Models.Enums;

namespace WebApiMezada.DTOs.FamilyGroup
{
    public class UserInfoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public EnumRoles Role { get; set; }
    }
}
