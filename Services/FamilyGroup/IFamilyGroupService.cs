using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.Models;

namespace WebApiMezada.Services.FamilyGroup
{
    public interface IFamilyGroupService
    {
        Task Join(string hashCode, string userId);
        Task<FamilyGroupModel> Create(FamilyGroupCreateDTO familyGroupDTO, string userId);
        Task<FamilyGroupModel> GetFamilyGroupById(string id);
        Task<FamilyGroupModel> Update(FamilyGroupModel familyGroup);

    }
}
