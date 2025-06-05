using WebApiMezada.DTOs.Task;
using WebApiMezada.DTOs.TaskHistory;
using WebApiMezada.Models;

namespace WebApiMezada.Services.TaskGroup
{
    public interface ITaskService
    {
        Task<TaskStatsDTO> GetTaskStats(string familyGroupId, string userId);
        Task<object> GetTaskById(string id, string userId);
        Task<List<TaskListDTO>> GetAll(string filter, string groupId, string userId);
        Task<List<FilterOptionDTO>> GetFilters(string groupId);
        Task<TaskModel> CreateOrUpdate(TaskCreateDTO taskDTO, string userId);
        Task<TaskModel> Update(TaskUpdateDTO taskDTO, string userId);
        Task AccountPoints(AccountPointsDTO dto, string userId);
        Task Delete(string taskId, string userId);
        Task<List<TaskHistoryListDTO>> GetTaskHistory(string taskId, string userId);
        Task RevertHistory(RevertHistoryDTO dto, string userId);
        Task EndCycle(string groupId, string userId);
        Task<CycleSummaryDTO> GetCycleSummary(string groupId, string userId);
        Task RemoveChildFromTask(RemoveChildFromTaskDTO dto, string userId);
    }
}