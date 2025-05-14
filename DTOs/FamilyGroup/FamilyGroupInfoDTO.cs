namespace WebApiMezada.DTOs.FamilyGroup
{
    public class FamilyGroupInfoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string HashCode { get; set; } = string.Empty;
        public List<UserInfoDTO> Users { get; set; } = new List<UserInfoDTO>();
    }
}
