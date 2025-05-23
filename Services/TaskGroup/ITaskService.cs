using WebApiMezada.DTOs.Task;
using WebApiMezada.DTOs.TaskHistory;
using WebApiMezada.Models;

namespace WebApiMezada.Services.TaskGroup
{
    public interface ITaskService
    {
        Task<TaskStatsDTO> GetTaskStats(string familyGroupId);
        Task<TaskModel> GetTaskById(string id);
        Task<List<TaskListDTO>> GetAll(string filter, string groupId, string userId);
        Task<List<FilterOptionDTO>> GetFilters(string groupId);
        Task<TaskModel> Create(TaskCreateDTO taskDTO, string userId);
        Task<TaskModel> Update(TaskUpdateDTO taskDTO, string userId);
        Task AccountPoints(AccountPointsDTO dto, string userId);
        Task Delete(string taskId, string userId);
        Task<List<TaskHistoryListDTO>> GetTaskHistory(string taskId, string userId);
        Task RevertHistory(RevertHistoryDTO dto, string userId);
        Task EndCycle(string groupId, string userId);
        Task<CycleSummaryDTO> GetCycleSummary(string groupId, string userId);
    }
}