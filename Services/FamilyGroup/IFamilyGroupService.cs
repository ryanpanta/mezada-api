using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.Models;

namespace WebApiMezada.Services.FamilyGroup
{
    public interface IFamilyGroupService
    {
        Task Join(string hashCode, string userId);
        Task<FamilyGroupModel> Create(FamilyGroupCreateDTO familyGroupDTO, string userId);
        Task<FamilyGroupInfoDTO> GetGroupInfo(string id, string userId);
        Task<FamilyGroupModel> GetFamilyGroupById(string id);
        Task<FamilyGroupModel> Update(FamilyGroupModel familyGroup);
        Task<string> SetAdmin(string groupId, string userIdToPromote, string currentUserId);
    }
}