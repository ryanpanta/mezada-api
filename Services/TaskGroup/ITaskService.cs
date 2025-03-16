using WebApiMezada.DTOs.Task;
using WebApiMezada.Models;

namespace WebApiMezada.Services.TaskGroup
{
    public interface ITaskService
    {
        Task<TaskModel> GetTaskById(string id);
        Task<List<TaskModel>> GetAll(int? statusFilter);
        Task<TaskModel> Create(TaskCreateDTO taskDTO, string userId);
        Task SetAsApproved(string taskId, string parentUserId);
        Task SetAsRejected(string taskId, string parentUserId);
        Task Delete(string taskId, string userId);
    }
}
