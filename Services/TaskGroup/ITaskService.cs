using System.Globalization;
using WebApiMezada.DTOs.Task;
using WebApiMezada.Models;

namespace WebApiMezada.Services.TaskGroup
{
    public interface ITaskService
    {
        Task<TaskModel> GetTaskById(string id);
        Task<List<TaskListDTO>> GetAll(string filter, string groupId, string userId);
        Task<List<FilterOptionDTO>> GetFilters(string groupId);
        Task<TaskModel> Create(TaskCreateDTO taskDTO, string userId);
        Task Delete(string taskId, string userId);
        Task<TaskStatsDTO> GetTaskStats(string familyGroupId);
    }
}
