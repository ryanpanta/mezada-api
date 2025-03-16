using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.Models.Enums;
using WebApiMezada.Models;
using WebApiMezada.Services.User;
using WebApiMezada.DTOs.Task;
using FluentValidation;

namespace WebApiMezada.Services.TaskGroup
{
    public class TaskService : ITaskService
    {

       
        private readonly IMongoCollection<TaskModel> _taskCollection;
        private readonly IUserService _userService;
        private readonly IValidator<TaskCreateDTO> _validator;

        public TaskService(IOptions<TaskDatabaseSettings> tasksSettings, IUserService userService, IValidator<TaskCreateDTO> validator)
            {
                var client = new MongoClient(tasksSettings.Value.ConnectionString);
                var database = client.GetDatabase(tasksSettings.Value.DatabaseName);
                _taskCollection = database.GetCollection<TaskModel>(tasksSettings.Value.TaskCollectionName);
                _userService = userService;
                _validator = validator;
        }

        public async Task<TaskModel> GetTaskById(string id)
        {
            var task = await _taskCollection
                .Find(t => t.Id == id)
                .FirstOrDefaultAsync();

            return task ?? throw new KeyNotFoundException("Tarefa não encontrada.");
        }

        public async Task<List<TaskModel>> GetAll(int? statusFilter)
        {
            var filter = Builders<TaskModel>.Filter.Empty;

            if (statusFilter.HasValue)
            {
                if (!Enum.IsDefined(typeof(EnumTaskStatus), statusFilter.Value))
                    throw new ArgumentException("Status inválido.", nameof(statusFilter));

                filter = Builders<TaskModel>.Filter.Eq(t => t.Status, (EnumTaskStatus)statusFilter.Value);
            }

            return await _taskCollection
                .Find(filter)
                .ToListAsync();
        }

        public async Task<TaskModel> Create(TaskCreateDTO taskDTO, string userId)
        {
            ValidateUserId(userId);
            var validationResult = _validator.Validate(taskDTO);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var user = await GetUserOrThrow(userId);
            EnsureUserHasFamilyGroup(user);

            var task = new TaskModel
            {
                Title = taskDTO.Title,
                Description = taskDTO.Description,
                Points = taskDTO.Points,
                UserId = userId,
                FamilyGroupId = user.FamilyGroupId
            };

            await _taskCollection.InsertOneAsync(task);
            UpdateUserTask(user, task.Id);

            return task;
        }

        public async Task SetAsApproved(string taskId, string parentUserId)
        {
            ValidateUserId(parentUserId);
            var task = await GetTaskOrThrow(taskId);
            var parent = await GetUserOrThrow(parentUserId);

            EnsureUserIsParent(parent);
            EnsureTaskBelongsToFamilyGroup(task, parent);

            task.Status = EnumTaskStatus.Approved;
            await _taskCollection.ReplaceOneAsync(t => t.Id == taskId, task);
        }

        public async Task SetAsRejected(string taskId, string parentUserId)
        {
            ValidateUserId(parentUserId);
            var task = await GetTaskOrThrow(taskId);
            var parent = await GetUserOrThrow(parentUserId);

            EnsureUserIsParent(parent);
            EnsureTaskBelongsToFamilyGroup(task, parent);

            task.Status = EnumTaskStatus.Rejected;
            await _taskCollection.ReplaceOneAsync(t => t.Id == taskId, task);
        }

        public async Task Delete(string taskId, string userId)
        {
            ValidateUserId(userId);
            var task = await GetTaskOrThrow(taskId);

            if (task.UserId != userId)
                throw new UnauthorizedAccessException("Somente o criador da tarefa pode excluí-la.");

            await _taskCollection.DeleteOneAsync(t => t.Id == taskId);
        }

        private void ValidateUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("O ID do usuário é obrigatório.", nameof(userId));
        }

        private async Task<UserModel> GetUserOrThrow(string userId)
        {
            var user = await _userService.GetUserById(userId);
            return user ?? throw new KeyNotFoundException("Usuário não encontrado.");
        }

        private void EnsureUserHasFamilyGroup(UserModel user)
        {
            if (string.IsNullOrEmpty(user.FamilyGroupId))
                throw new InvalidOperationException("O usuário deve pertencer a um grupo familiar para criar uma tarefa.");
        }

        private void EnsureUserIsParent(UserModel user)
        {
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Somente usuários com papel 'Parent' podem aprovar ou rejeitar tarefas.");
        }

        private void EnsureTaskBelongsToFamilyGroup(TaskModel task, UserModel parent)
        {
            if (task.FamilyGroupId != parent.FamilyGroupId)
                throw new UnauthorizedAccessException("A tarefa não pertence ao grupo familiar do usuário.");
        }

        private async Task<TaskModel> GetTaskOrThrow(string taskId)
        {
            var task = await GetTaskById(taskId);
            return task ?? throw new KeyNotFoundException("Tarefa não encontrada.");
        }

        private async void UpdateUserTask(UserModel user, string taskId)
        {
            user.Tasks.Add(taskId);
            await _userService.Update(user);
        }

    }

}
