using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.Models;

namespace WebApiMezada.Services.FamilyGroup
{
    public interface ICycleService
    {
        Task<CycleModel> GetCurrentCycleByGroupId(string familyGroupId);
    }
}
