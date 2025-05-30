using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.DTOs.Task;
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
        Task<List<ChildUserDTO>> GetChildrenInGroup(string groupId, string userId);
    }
}