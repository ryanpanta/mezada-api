using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.Models;

namespace WebApiMezada.Services.FamilyGroup
{
    public interface ITaskHistoryService
    {
        Task<CycleModel> GetCurrentCycleByGroupId(string familyGroupId);
    }
}
