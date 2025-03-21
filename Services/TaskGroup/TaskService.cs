﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.Models.Enums;
using WebApiMezada.Models;
using WebApiMezada.Services.User;
using WebApiMezada.DTOs.Task;
using FluentValidation;
using WebApiMezada.Services.FamilyGroup;

namespace WebApiMezada.Services.TaskGroup
{
    public class TaskService : ITaskService
    {

       
        private readonly IMongoCollection<TaskModel> _taskCollection;
        private readonly IUserService _userService;
        private readonly IFamilyGroupService _familyGroupService;
        private readonly IValidator<TaskCreateDTO> _validator;

        public TaskService(IOptions<TaskDatabaseSettings> tasksSettings, IUserService userService, IValidator<TaskCreateDTO> validator, IFamilyGroupService familyGroupService)
            {
                var client = new MongoClient(tasksSettings.Value.ConnectionString);
                var database = client.GetDatabase(tasksSettings.Value.DatabaseName);
                _taskCollection = database.GetCollection<TaskModel>(tasksSettings.Value.TaskCollectionName);
                _userService = userService;
                _familyGroupService = familyGroupService;
            _validator = validator;
        }

        public async Task<TaskStatsDTO> GetTaskStats(string familyGroupId)
        {
            //get thet asks where is active and familyGroupId is equal to the familyGroupId
            var tasks = await _taskCollection.Find(t => t.Active == true && t.FamilyGroupId == familyGroupId).ToListAsync();
            var total = tasks.Count;
            var approved = tasks.Count(t => t.Status == EnumTaskStatus.Approved);
            var rejected = tasks.Count(t => t.Status == EnumTaskStatus.Rejected);
            var pending = tasks.Count(t => t.Status == EnumTaskStatus.Pending);

            return new TaskStatsDTO
            {
                Total = total,
                Approved = approved,
                Rejected = rejected,
                Pending = pending
            };

        }

        public async Task<TaskModel> GetTaskById(string id)
        {
            var task = await _taskCollection
                .Find(t => t.Id == id)
                .FirstOrDefaultAsync();

            return task ?? throw new KeyNotFoundException("Tarefa não encontrada.");
        }

        public async Task<List<TaskListDTO>> GetAll(int? statusFilter, string familyGroupId)
        {
            var filter = Builders<TaskModel>.Filter.Eq(t => t.FamilyGroupId, familyGroupId);

            if (statusFilter.HasValue)
            {
                if (!Enum.IsDefined(typeof(EnumTaskStatus), statusFilter.Value))
                    throw new ArgumentException("Status inválido.", nameof(statusFilter));

                var statusFilterDef = Builders<TaskModel>.Filter.Eq(t => t.Status, (EnumTaskStatus)statusFilter.Value);
                filter = Builders<TaskModel>.Filter.And(filter, statusFilterDef);
            }

            var tasks = await _taskCollection
                .Find(filter)
                .ToListAsync();

            return tasks.Select(t => new TaskListDTO
            {
                Active = t.Active,
                FamilyGroupId = t.FamilyGroupId,
                UserId = t.UserId,
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Points = t.Points,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                UserName = _userService.GetUserById(t.UserId).Result.Name
            }).ToList();
        }

        public async Task<TaskModel> Create(TaskCreateDTO taskDTO, string userId)
        {
            ValidateUserId(userId);
            var validationResult = _validator.Validate(taskDTO);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var user = await GetUserOrThrow(userId);
            EnsureUserHasFamilyGroup(user);

            var familyGroup = await GeFamilyGroupOrThrow(user.FamilyGroupId);

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
            UpdateFamilyGroupTask(familyGroup, task.Id);

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

        private async Task<FamilyGroupModel> GeFamilyGroupOrThrow(string familyGroupId)
        {
            var familyGroup = await _familyGroupService.GetFamilyGroupById(familyGroupId);
            return familyGroup ?? throw new KeyNotFoundException("Grupo familiar não encontrado.");
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

        private async void UpdateFamilyGroupTask(FamilyGroupModel familyGroup, string taskId)
        {
            familyGroup.Tasks.Add(taskId);
            await _familyGroupService.Update(familyGroup);
        }

    }

}
