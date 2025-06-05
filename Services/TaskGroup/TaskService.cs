using FluentValidation;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.DTOs.Task;
using WebApiMezada.DTOs.TaskHistory;
using WebApiMezada.Models;
using WebApiMezada.Models.Enums;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.User;

namespace WebApiMezada.Services.TaskGroup
{
    public class TaskService : ITaskService
    {
        private readonly IMongoCollection<TaskModel> _taskCollection;
        private readonly IMongoCollection<TaskAssignmentModel> _taskAssignmentCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IMongoCollection<TaskHistoryModel> _taskHistoryCollection;
        private readonly IMongoCollection<CycleModel> _cycleCollection;
        private readonly IUserService _userService;
        private readonly IFamilyGroupService _familyGroupService;
        private readonly ICycleService _cycleService;
        private readonly IValidator<TaskCreateDTO> _validator;
        private readonly IValidator<TaskUpdateDTO> _updateValidator;


        public TaskService(IOptions<TaskDatabaseSettings> tasksSettings, IUserService userService,
            IValidator<TaskCreateDTO> validator, IFamilyGroupService familyGroupService, ICycleService cycleService,
            IOptions<TaskAssignmentDatabaseSettings> taskAssignmentSettings,
            IOptions<UserDatabaseSettings> userSettings, IValidator<TaskUpdateDTO> updateValidator,
            IOptions<TaskHistoryDatabaseSettings> taskHistorySettings, IOptions<CycleDatabaseSettings> cycleSettings)
        {
            var client = new MongoClient(tasksSettings.Value.ConnectionString);
            var database = client.GetDatabase(tasksSettings.Value.DatabaseName);
            _taskCollection = database.GetCollection<TaskModel>(tasksSettings.Value.TaskCollectionName);
            _taskAssignmentCollection =
                database.GetCollection<TaskAssignmentModel>(taskAssignmentSettings.Value.TaskAssignmentCollectionName);
            _userCollection = database.GetCollection<UserModel>(userSettings.Value.UserCollectionName);
            _taskHistoryCollection =
                database.GetCollection<TaskHistoryModel>(taskHistorySettings.Value.TaskHistoryCollectionName);
            _cycleCollection =
                database.GetCollection<CycleModel>(cycleSettings.Value.CycleCollectionName);
            _userService = userService;
            _familyGroupService = familyGroupService;
            _validator = validator;
            _cycleService = cycleService;
            _updateValidator = updateValidator;
        }

        public async Task<TaskStatsDTO> GetTaskStats(string familyGroupId, string userId)
        {
            var user = await GetUserOrThrow(userId);
            if (user.FamilyGroupId != familyGroupId)
                throw new UnauthorizedAccessException("Usuário não pertence a este grupo.");

            TaskStatsDTO stats;

            if (user.Role == EnumRoles.Parent)
            {
                var tasks = await _taskCollection
                    .Find(t => t.Active == true && t.FamilyGroupId == familyGroupId)
                    .ToListAsync();

                var total = tasks.Count;
                var rewards = tasks.Count(t => t.Category == EnumCategory.Reward);
                var penalties = tasks.Count(t => t.Category == EnumCategory.Penalty);

                stats = new TaskStatsDTO
                {
                    Total = total,
                    Rewards = rewards,
                    Penalties = penalties
                };
            }
            else if (user.Role == EnumRoles.Child)
            {
                var assignments = await _taskAssignmentCollection
                    .Find(ta => ta.ChildId == userId && !ta.IsDeleted)
                    .ToListAsync();

                var taskIds = assignments.Select(ta => ta.TaskId).ToList();
                var tasks = await _taskCollection
                    .Find(t => t.Active == true && t.FamilyGroupId == familyGroupId && taskIds.Contains(t.Id))
                    .ToListAsync();

                var total = tasks.Count;
                var rewards = tasks.Count(t => t.Category == EnumCategory.Reward);
                var penalties = tasks.Count(t => t.Category == EnumCategory.Penalty);

                stats = new TaskStatsDTO
                {
                    Total = total,
                    Rewards = rewards,
                    Penalties = penalties
                };
            }
            else
            {
                throw new InvalidOperationException("Papel do usuário não reconhecido.");
            }

            return stats;
        }

        public async Task<object> GetTaskById(string id, string userId)
        {
            var user = await GetUserOrThrow(userId);
            var task = await _taskCollection.Find(t => t.Id == id && t.Active).FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            if (task.FamilyGroupId != user.FamilyGroupId)
                throw new UnauthorizedAccessException("Você não tem acesso a esta tarefa.");

            if (user.Role == EnumRoles.Parent)
            {
                var assignments = await _taskAssignmentCollection
                    .Find(ta => ta.TaskId == id && !ta.IsDeleted)
                    .ToListAsync();

                var childrenDetails = new List<ChildTaskDetailsDTO>();
                foreach (var assignment in assignments)
                {
                    var child = await _userCollection.Find(u => u.Id == assignment.ChildId).FirstOrDefaultAsync();
                    if (child != null)
                    {
                        childrenDetails.Add(new ChildTaskDetailsDTO
                        {
                            ChildId = assignment.ChildId,
                            ChildName = child.Name,
                            CurrentBalance = assignment.CurrentBalance,
                            BonusBalance = assignment.BonusBalance,
                            CustomIncrement = assignment.CustomIncrement
                        });
                    }
                }

                return new TaskDetailsForParentDTO()
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Category = task.Category.ToString(),
                    InitialValue = task.InitialValue,
                    DefaultIncrement = task.DefaultIncrement,
                    LimitValue = task.LimitValue,
                    Children = childrenDetails
                };
            }
            // Se o usuário for filho, retornar visão limitada
            else
            {
                var assignment = await _taskAssignmentCollection
                    .Find(ta => ta.TaskId == id && ta.ChildId == userId && !ta.IsDeleted)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                    throw new UnauthorizedAccessException("Você não está associado a esta tarefa.");

                return new TaskDetailsForChildDTO
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Category = task.Category.ToString(),
                    InitialValue = task.InitialValue,
                    DefaultIncrement = task.DefaultIncrement,
                    LimitValue = task.LimitValue,
                    CurrentBalance = assignment.CurrentBalance,
                    BonusBalance = assignment.BonusBalance
                };
            }
        }

        public async Task<List<TaskListDTO>> GetAll(string? filter, string groupId, string userId)
        {
            var isParent = (await _userService.GetUserById(userId))?.Role == EnumRoles.Parent;

            var activeCycle = await _cycleService.GetCurrentCycleByGroupId(groupId);

            if (activeCycle is null)
                return new List<TaskListDTO>();

            var filterBuilder = Builders<TaskModel>.Filter;
            var filterDefinition = filterBuilder.Eq(t => t.FamilyGroupId, groupId) &
                                   filterBuilder.Eq(t => t.CycleId, activeCycle.Id) &
                                   filterBuilder.Eq(t => t.Active, true);

            if (isParent)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (filter.ToLower() == "penalties")
                        filterDefinition &= filterBuilder.Eq(t => t.Category, EnumCategory.Penalty);
                    else if (filter.ToLower() == "rewards")
                        filterDefinition &= filterBuilder.Eq(t => t.Category, EnumCategory.Reward);
                    else if (ObjectId.TryParse(filter, out _))
                    {
                        var childAssignments = await _taskAssignmentCollection
                            .Find(ta => ta.ChildId == filter && !ta.IsDeleted)
                            .Project(ta => ta.TaskId)
                            .ToListAsync();

                        if (childAssignments.Any())
                            filterDefinition &= filterBuilder.In(t => t.Id, childAssignments);
                        else
                            return new List<TaskListDTO>();
                    }
                }
            }
            else
            {
                // Para filhos, filtrar apenas as tarefas que eles estão associados
                var childAssignments = await _taskAssignmentCollection
                    .Find(ta => ta.ChildId == userId && !ta.IsDeleted)
                    .Project(ta => ta.TaskId)
                    .ToListAsync();

                if (childAssignments.Any())
                    filterDefinition &= filterBuilder.In(t => t.Id, childAssignments);
                else
                    return new List<TaskListDTO>();
            }

            var tasks = await _taskCollection
                .Find(filterDefinition)
                .ToListAsync();

            var taskDtos = new List<TaskListDTO>();
            foreach (var task in tasks)
            {
                var childIds = await _taskAssignmentCollection
                    .Find(ta => ta.TaskId == task.Id && !ta.IsDeleted)
                    .Project(ta => ta.ChildId)
                    .ToListAsync();

                var creator = await _userService.GetUserById(task.UserId);
                var creatorName = creator?.Name ?? "Desconhecido";

                taskDtos.Add(new TaskListDTO
                {
                    Id = task.Id,
                    Title = task.Title,
                    Category = task.Category,
                    CreatedAt = task.CreatedAt,
                    Active = task.Active,
                    FamilyGroupId = task.FamilyGroupId,
                    ChildIds = childIds,
                    CreatorName = creatorName,
                    IsIncreasing = task.Category == EnumCategory.Reward
                });
            }

            return taskDtos;
        }

        public async Task<List<FilterOptionDTO>> GetFilters(string groupId)
        {
            var filters = new List<FilterOptionDTO>
            {
                new FilterOptionDTO { Value = "penalties", Label = "Penalidades" },
                new FilterOptionDTO { Value = "rewards", Label = "Recompensas" }
            };

            var children = await _userCollection
                .Find(u => u.FamilyGroupId == groupId && u.Role == EnumRoles.Child && u.Active)
                .Project(u => new { u.Id, u.Name })
                .ToListAsync();

            filters.AddRange(children.Select(c => new FilterOptionDTO
            {
                Value = c.Id,
                Label = c.Name
            }));

            return filters;
        }

        public async Task<TaskModel> CreateOrUpdate(TaskCreateDTO taskDTO, string userId)
        {
            ValidateUserId(userId);
            var validationResult = _validator.Validate(taskDTO);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem criar tarefas.");

            EnsureUserHasFamilyGroup(user);
            var familyGroup = await GeFamilyGroupOrThrow(user.FamilyGroupId);

            var activeCycle = await _cycleService.GetCurrentCycleByGroupId(user.FamilyGroupId);

            if (activeCycle == null)
                throw new InvalidOperationException("Nenhum ciclo ativo encontrado para o grupo.");

            TaskModel task;
            bool isUpdate = !string.IsNullOrEmpty(taskDTO.Id);

            if (isUpdate)
            {
                // Atualização
                task = await _taskCollection.Find(t => t.Id == taskDTO.Id && t.Active).FirstOrDefaultAsync();
                if (task == null)
                    throw new KeyNotFoundException("Tarefa não encontrada.");

                task.Title = taskDTO.Title;
                task.Description = taskDTO.Description;
                task.Category = taskDTO.Category;
                task.DefaultIncrement = taskDTO.DefaultIncrement;
                task.LimitValue = taskDTO.Category == EnumCategory.Reward ? taskDTO.LimitValue : 0;

                await _taskCollection.ReplaceOneAsync(t => t.Id == task.Id, task);
            }
            else
            {
                task = new TaskModel
                {
                    CycleId = activeCycle.Id,
                    UserId = userId,
                    FamilyGroupId = user.FamilyGroupId,
                    Title = taskDTO.Title,
                    Description = taskDTO.Description,
                    Category = taskDTO.Category,
                    InitialValue = taskDTO.InitialValue,
                    DefaultIncrement = taskDTO.DefaultIncrement,
                    LimitValue = taskDTO.Category == EnumCategory.Reward ? taskDTO.LimitValue : 0
                };

                await _taskCollection.InsertOneAsync(task);
            }

            if (isUpdate)
            {
                await _taskAssignmentCollection.UpdateManyAsync(
                    ta => ta.TaskId == task.Id && !ta.IsDeleted,
                    Builders<TaskAssignmentModel>.Update.Set(ta => ta.IsDeleted, true)
                );
            }

            foreach (var childAssignment in taskDTO.Children)
            {
                var child = await _userCollection
                    .Find(u => u.Id == childAssignment.ChildId && u.FamilyGroupId == user.FamilyGroupId &&
                               u.Role == EnumRoles.Child)
                    .FirstOrDefaultAsync();
                if (child == null)
                    continue;

                var existingAssignment = await _taskAssignmentCollection
                    .Find(ta => ta.TaskId == task.Id && ta.ChildId == childAssignment.ChildId)
                    .FirstOrDefaultAsync();

                if (existingAssignment != null)
                {
                    // Restaurar a atribuição (caso tenha sido marcada como deletada) e atualizar o CustomIncrement
                    await _taskAssignmentCollection.UpdateOneAsync(
                        ta => ta.Id == existingAssignment.Id,
                        Builders<TaskAssignmentModel>.Update
                            .Set(ta => ta.IsDeleted, false)
                            .Set(ta => ta.CustomIncrement, childAssignment.CustomIncrement)
                    );
                }
                else
                {
                    // Criar nova atribuição
                    var assignment = new TaskAssignmentModel
                    {
                        TaskId = task.Id,
                        ChildId = childAssignment.ChildId,
                        CustomIncrement = childAssignment.CustomIncrement,
                        CurrentBalance = task.Category == EnumCategory.Penalty ? task.InitialValue : 0,
                        BonusBalance = 0,
                        IsDeleted = false
                    };
                    await _taskAssignmentCollection.InsertOneAsync(assignment);
                }
            }

            return task;
        }

        public async Task<TaskModel> Update(TaskUpdateDTO taskDTO, string userId)
        {
            ValidateUserId(userId);
            var validationResult = _updateValidator.Validate(taskDTO);
            if (!validationResult.IsValid)
                throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var user = await GetUserOrThrow(userId);

            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem editar tarefas.");

            var task = await _taskCollection
                .Find(t => t.Id == taskDTO.Id && t.FamilyGroupId == user.FamilyGroupId && t.Active)
                .FirstOrDefaultAsync();

            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            var initialValue = taskDTO.Category == EnumCategory.Reward ? 0 : taskDTO.DefaultIncrement;

            task.Title = taskDTO.Title;
            task.Description = taskDTO.Description;
            task.Category = taskDTO.Category;
            task.InitialValue = initialValue;
            task.DefaultIncrement = taskDTO.Category == EnumCategory.Reward
                ? Math.Abs(taskDTO.DefaultIncrement)
                : -Math.Abs(taskDTO.DefaultIncrement);
            task.LimitValue = taskDTO.Category == EnumCategory.Reward ? taskDTO.LimitValue : 0;

            await _taskCollection.ReplaceOneAsync(t => t.Id == task.Id, task);

            // Atualizar os filhos associados
            await _taskAssignmentCollection.UpdateManyAsync(
                ta => ta.TaskId == task.Id,
                Builders<TaskAssignmentModel>.Update.Set(ta => ta.IsDeleted, true)
            );

            foreach (var childId in taskDTO.ChildIds)
            {
                var child = await _userCollection
                    .Find(u => u.Id == childId && u.FamilyGroupId == user.FamilyGroupId && u.Role == EnumRoles.Child)
                    .FirstOrDefaultAsync();

                if (child == null)
                    continue;

                var existingAssignment = await _taskAssignmentCollection
                    .Find(ta => ta.TaskId == task.Id && ta.ChildId == childId && ta.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingAssignment != null)
                {
                    await _taskAssignmentCollection.UpdateOneAsync(
                        ta => ta.Id == existingAssignment.Id,
                        Builders<TaskAssignmentModel>.Update
                            .Set(ta => ta.IsDeleted, false)
                            .Set(ta => ta.CustomIncrement, task.DefaultIncrement)
                    );
                }
                else
                {
                    var assignment = new TaskAssignmentModel
                    {
                        TaskId = task.Id,
                        ChildId = childId,
                        CustomIncrement = task.DefaultIncrement,
                        CurrentBalance = 0,
                        BonusBalance = 0,
                        IsDeleted = false
                    };
                    await _taskAssignmentCollection.InsertOneAsync(assignment);
                }
            }

            return task;
        }

        public async Task AccountPoints(AccountPointsDTO dto, string userId)
        {
            ValidateUserId(userId);
            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem contabilizar pontos.");

            var task = await _taskCollection
                .Find(t => t.Id == dto.TaskId && t.FamilyGroupId == user.FamilyGroupId && t.Active)
                .FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            var assignment = await _taskAssignmentCollection
                .Find(ta => ta.TaskId == dto.TaskId && ta.ChildId == dto.ChildId && !ta.IsDeleted)
                .FirstOrDefaultAsync();

            if (assignment == null)
                throw new KeyNotFoundException("Atribuição não encontrada para o filho.");

            if (task.Category == EnumCategory.Reward)
            {
                // Para recompensas, incrementa o CustomIncrement no CurrentBalance
                assignment.CurrentBalance += assignment.CustomIncrement;
                if (assignment.CurrentBalance > task.LimitValue && task.LimitValue > 0)
                {
                    var excess = assignment.CurrentBalance - task.LimitValue;
                    assignment.CurrentBalance = task.LimitValue;
                    assignment.BonusBalance += excess;
                }
            }
            else if (task.Category == EnumCategory.Penalty)
            {
                var newBalance = assignment.CurrentBalance - assignment.CustomIncrement;
                if (newBalance < 0)
                {
                    assignment.BonusBalance += newBalance; // Saldo negativo vai para BonusBalance
                    assignment.CurrentBalance = 0;
                }
                else
                {
                    assignment.CurrentBalance = newBalance;
                }
            }

            await _taskAssignmentCollection.ReplaceOneAsync(ta => ta.Id == assignment.Id, assignment);

            var history = new TaskHistoryModel
            {
                TaskId = dto.TaskId,
                AssignmentId = assignment.Id,
                AccountedBy = userId,
                ChildId = dto.ChildId,
                Value = assignment.CustomIncrement,
                CreatedAt = DateTime.UtcNow,
                IsReverted = false,
                IsDeleted = false
            };
            await _taskHistoryCollection.InsertOneAsync(history);
        }

        public async Task Delete(string taskId, string userId)
        {
            ValidateUserId(userId);
            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Somente os pais podem excluir esta tarefa.");

            var task = await _taskCollection
                .Find(t => t.Id == taskId && t.FamilyGroupId == user.FamilyGroupId && t.Active).FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            await _taskCollection.UpdateOneAsync(
                t => t.Id == taskId,
                Builders<TaskModel>.Update.Set(t => t.Active, false)
            );

            // excluir todas as atribuições associadas
            await _taskAssignmentCollection.UpdateManyAsync(
                ta => ta.TaskId == taskId,
                Builders<TaskAssignmentModel>.Update.Set(ta => ta.IsDeleted, true)
            );
        }

        public async Task<List<TaskHistoryListDTO>> GetTaskHistory(string taskId, string userId)
        {
            var user = await GetUserOrThrow(userId);
            var task = await _taskCollection
                .Find(t => t.Id == taskId && t.FamilyGroupId == user.FamilyGroupId && t.Active)
                .FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            var history = await _taskHistoryCollection
                .Find(h => h.TaskId == taskId && !h.IsDeleted)
                .ToListAsync();

            var historyDtos = new List<TaskHistoryListDTO>();
            foreach (var item in history)
            {
                var child = await _userCollection.Find(u => u.Id == item.ChildId).FirstOrDefaultAsync();
                var accountedBy = await _userCollection.Find(u => u.Id == item.AccountedBy).FirstOrDefaultAsync();

                historyDtos.Add(new TaskHistoryListDTO
                {
                    Id = item.Id,
                    TaskId = item.TaskId,
                    ChildId = item.ChildId,
                    ChildName = child?.Name ?? "Desconhecido",
                    AccountedById = item.AccountedBy,
                    AccountedByName = accountedBy?.Name ?? "Desconhecido",
                    Value = item.Value,
                    AccountedAt = item.CreatedAt,
                    IsReverted = item.IsReverted
                });
            }

            return historyDtos.OrderByDescending(h => h.AccountedAt).ToList();
        }

        public async Task RevertHistory(RevertHistoryDTO dto, string userId)
        {
            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem reverter contabilizações.");

            var history = await _taskHistoryCollection
                .Find(h => h.Id == dto.HistoryId && !h.IsReverted && !h.IsDeleted)
                .FirstOrDefaultAsync();
            if (history == null)
                throw new KeyNotFoundException("Histórico não encontrado ou já revertido.");

            var assignment = await _taskAssignmentCollection
                .Find(ta => ta.TaskId == history.TaskId && ta.ChildId == history.ChildId && !ta.IsDeleted)
                .FirstOrDefaultAsync();
            if (assignment == null)
                throw new KeyNotFoundException("Atribuição não encontrada.");

            var task = await _taskCollection.Find(t => t.Id == history.TaskId).FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            var limitValue = task.LimitValue;
            var newBalance = assignment.CurrentBalance;

            // Reverter com base na categoria da tarefa
            if (task.Category == EnumCategory.Reward)
            {
                // Para recompensas, subtrai o valor do histórico do CurrentBalance
                newBalance -= history.Value;
                var updatedBalance = Math.Min(Math.Max(newBalance, 0), limitValue); // Limita entre 0 e LimitValue
                var bonusBalance =
                    Math.Max(0, newBalance - limitValue); // Ajusta o BonusBalance para valores excedentes
                await _taskAssignmentCollection.UpdateOneAsync(
                    ta => ta.Id == assignment.Id,
                    Builders<TaskAssignmentModel>.Update
                        .Set(ta => ta.CurrentBalance, updatedBalance)
                        .Set(ta => ta.BonusBalance, bonusBalance)
                );
            }
            else if (task.Category == EnumCategory.Penalty)
            {
                // Para penalidades, soma o valor do histórico ao CurrentBalance (reverte a subtração)
                newBalance += history.Value;
                var updatedBalance = Math.Max(newBalance, 0); // Limita o mínimo a 0 (ou outro valor se necessário)
                var bonusBalance = Math.Min(0, newBalance); // Ajusta o BonusBalance para valores negativos
                await _taskAssignmentCollection.UpdateOneAsync(
                    ta => ta.Id == assignment.Id,
                    Builders<TaskAssignmentModel>.Update
                        .Set(ta => ta.CurrentBalance, updatedBalance)
                        .Set(ta => ta.BonusBalance, bonusBalance)
                );
            }

            // Marcar o histórico como revertido
            await _taskHistoryCollection.UpdateOneAsync(
                h => h.Id == history.Id,
                Builders<TaskHistoryModel>.Update.Set(h => h.IsReverted, true)
            );
        }

        public async Task<CycleSummaryDTO> GetCycleSummary(string groupId, string userId)
        {
            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem visualizar o resumo do ciclo.");

            var activeCycle = await _cycleCollection
                .Find(c => c.FamilyGroupId == groupId && c.IsActive)
                .FirstOrDefaultAsync();
            if (activeCycle == null)
                throw new InvalidOperationException("Nenhum ciclo ativo encontrado.");

            var tasks = await _taskCollection
                .Find(t => t.FamilyGroupId == groupId && t.CycleId == activeCycle.Id && t.Active)
                .ToListAsync();

            var taskIds = tasks.Select(t => t.Id).ToList();
            var assignments = await _taskAssignmentCollection
                .Find(ta => taskIds.Contains(ta.TaskId) && !ta.IsDeleted)
                .ToListAsync();

            var taskHistories = await _taskHistoryCollection
                .Find(h => taskIds.Contains(h.TaskId) && !h.IsDeleted && !h.IsReverted)
                .ToListAsync();

            var childrenIds = assignments.Select(ta => ta.ChildId).Distinct().ToList();
            var children = await _userCollection
                .Find(u => childrenIds.Contains(u.Id))
                .ToListAsync();

            var summary = new CycleSummaryDTO
            {
                GroupId = groupId,
                CycleId = activeCycle.Id,
                TotalPositiveBalance = 0,
                TotalNegativeBalance = 0,
                ChildrenSummaries = new List<ChildCycleSummaryDTO>()
            };

            // Calcular o resumo por filho
            foreach (var childId in childrenIds)
            {
                var child = children.FirstOrDefault(c => c.Id == childId);
                if (child == null) continue;

                var childAssignments = assignments.Where(ta => ta.ChildId == childId).ToList();
                var childHistories =
                    taskHistories.Where(h => h.ChildId == childId).OrderBy(h => h.CreatedAt).ToList();

                int currentBalanceTotal = 0, bonusBalanceTotal = 0, rewardBalance = 0, penaltyBalance = 0;

                // Calcular saldos por categoria usando apenas CurrentBalance
                foreach (var assignment in childAssignments)
                {
                    var task = tasks.First(t => t.Id == assignment.TaskId);
                    var currentBalance = assignment.CurrentBalance; // Apenas o saldo atual
                    var bonusBalance = assignment.BonusBalance; // Bônus separado

                    if (task.Category == EnumCategory.Reward)
                    {
                        rewardBalance += currentBalance;
                    }
                    else if (task.Category == EnumCategory.Penalty)
                    {
                        penaltyBalance += currentBalance;
                    }

                    currentBalanceTotal += currentBalance;
                    bonusBalanceTotal += bonusBalance;
                }

                var totalBalance = currentBalanceTotal; // Somente o saldo atual como sugestão de mesada
                if (totalBalance > 0)
                    summary.TotalPositiveBalance += totalBalance;
                else
                    summary.TotalNegativeBalance += totalBalance;

                // Calcular histórico de saldo
                var balanceHistory = new List<BalanceHistoryDTO>();
                int cumulativeBalance = 0;
                foreach (var history in childHistories)
                {
                    var task = tasks.First(t => t.Id == history.TaskId);
                    var value = task.Category == EnumCategory.Reward ? history.Value : -history.Value;
                    cumulativeBalance += value;
                    balanceHistory.Add(new BalanceHistoryDTO
                    {
                        Date = history.CreatedAt,
                        Balance = cumulativeBalance
                    });
                }

                summary.ChildrenSummaries.Add(new ChildCycleSummaryDTO
                {
                    ChildId = childId,
                    ChildName = child.Name,
                    TotalBalance = totalBalance, // Apenas CurrentBalance como mesada sugerida
                    TotalBonus = bonusBalanceTotal, // Bônus total separado
                    RewardBalance = rewardBalance, // Apenas CurrentBalance de recompensas
                    PenaltyBalance = penaltyBalance, // Apenas CurrentBalance de penalidades
                    BalanceHistory = balanceHistory
                });
            }

            return summary;
        }


        public async Task EndCycle(string groupId, string userId)
        {
            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem encerrar o ciclo.");

            var activeCycle = await _cycleCollection
                .Find(c => c.FamilyGroupId == groupId && c.IsActive)
                .FirstOrDefaultAsync();
            if (activeCycle == null)
                throw new InvalidOperationException("Nenhum ciclo ativo encontrado.");

            // Desativar o ciclo atual
            await _cycleCollection.UpdateOneAsync(
                c => c.Id == activeCycle.Id,
                Builders<CycleModel>.Update.Set(c => c.IsActive, false)
            );

            // Criar novo ciclo
            var newCycle = new CycleModel
            {
                FamilyGroupId = groupId,
                IsActive = true,
                StartDate = DateTime.UtcNow
            };
            await _cycleCollection.InsertOneAsync(newCycle);

            // Resetar saldos das atribuições
            var tasks = await _taskCollection
                .Find(t => t.FamilyGroupId == groupId && t.Active)
                .ToListAsync();

            foreach (var task in tasks)
            {
                await _taskAssignmentCollection.UpdateManyAsync(
                    ta => ta.TaskId == task.Id,
                    Builders<TaskAssignmentModel>.Update
                        .Set(ta => ta.CurrentBalance, 0)
                        .Set(ta => ta.BonusBalance, 0)
                );
            }

            // Atualizar as tarefas para o novo ciclo
            await _taskCollection.UpdateManyAsync(
                t => t.FamilyGroupId == groupId && t.Active,
                Builders<TaskModel>.Update.Set(t => t.CycleId, newCycle.Id)
            );
        }

        public async Task RemoveChildFromTask(RemoveChildFromTaskDTO dto, string userId)
        {
            var user = await GetUserOrThrow(userId);
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem remover filhos de tarefas.");

            var task = await _taskCollection.Find(t => t.Id == dto.TaskId && t.Active).FirstOrDefaultAsync();
            if (task == null)
                throw new KeyNotFoundException("Tarefa não encontrada.");

            var assignment = await _taskAssignmentCollection
                .Find(ta => ta.TaskId == dto.TaskId && ta.ChildId == dto.ChildId && !ta.IsDeleted)
                .FirstOrDefaultAsync();
            if (assignment == null)
                throw new KeyNotFoundException("Atribuição não encontrada para este filho e tarefa.");

            var activeCycle = await _cycleCollection
                .Find(c => c.FamilyGroupId == task.FamilyGroupId && c.IsActive)
                .FirstOrDefaultAsync();
            if (activeCycle == null)
                throw new InvalidOperationException("Nenhum ciclo ativo encontrado para o grupo.");

            if (task.CycleId != activeCycle.Id)
                throw new InvalidOperationException("A tarefa não pertence ao ciclo ativo atual.");

            await _taskAssignmentCollection.UpdateOneAsync(
                ta => ta.Id == assignment.Id,
                Builders<TaskAssignmentModel>.Update
                    .Set(ta => ta.IsDeleted, true)
            );
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
                throw new InvalidOperationException(
                    "O usuário deve pertencer a um grupo familiar para criar uma tarefa.");
        }

        private void EnsureUserIsParent(UserModel user)
        {
            if (user.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException(
                    "Somente usuários com papel 'Parent' podem aprovar ou rejeitar tarefas.");
        }

        private void EnsureTaskBelongsToFamilyGroup(TaskModel task, UserModel parent)
        {
            if (task.FamilyGroupId != parent.FamilyGroupId)
                throw new UnauthorizedAccessException("A tarefa não pertence ao grupo familiar do usuário.");
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