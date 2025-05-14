using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.Models;
using WebApiMezada.Models.Enums;
using WebApiMezada.Services.User;

namespace WebApiMezada.Services.FamilyGroup
{
    public class FamilyGroupService : IFamilyGroupService
    {
        private readonly IMongoCollection<FamilyGroupModel> _familyGroupCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IUserService _userService;


        public FamilyGroupService(IOptions<FamilyGroupDatabaseSettings> familyGroupsSettings, IUserService userService,
            IOptions<UserDatabaseSettings> userSettings)
        {
            var client = new MongoClient(familyGroupsSettings.Value.ConnectionString);
            var database = client.GetDatabase(familyGroupsSettings.Value.DatabaseName);
            _familyGroupCollection =
                database.GetCollection<FamilyGroupModel>(familyGroupsSettings.Value.FamilyGroupCollectionName);
            _userCollection = database.GetCollection<UserModel>(userSettings.Value.UserCollectionName);
            _userService = userService;
        }

        public async Task<FamilyGroupModel> GetFamilyGroupById(string id)
        {
            var familyGroup = await _familyGroupCollection.Find(familyGroup => familyGroup.Id == id)
                .FirstOrDefaultAsync();
            if (familyGroup is null)
            {
                throw new Exception("Grupo familiar não encontrado.");
            }

            return familyGroup;
        }

        public async Task<FamilyGroupInfoDTO> GetGroupInfo(string id, string userId)
        {
            var familyGroup = await _familyGroupCollection.Find(fg => fg.Id == id && fg.Active).FirstOrDefaultAsync();
            if (familyGroup == null)
                throw new KeyNotFoundException("Grupo não encontrado.");

            var userIds = familyGroup.Users;
            var users = await _userCollection.Find(u => userIds.Contains(u.Id) && u.Active)
                .Project(u => new UserInfoDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = u.Role
                })
                .ToListAsync();

            return new FamilyGroupInfoDTO
            {
                Id = familyGroup.Id,
                Name = familyGroup.Name,
                HashCode = familyGroup.HashCode,
                Users = users
            };
        }

        public async Task<FamilyGroupModel> Create(FamilyGroupCreateDTO familyGroupDTO, string userId)
        {
            if (string.IsNullOrEmpty(familyGroupDTO.Name))
            {
                throw new Exception("Nome do grupo familiar não pode ser vazio.");
            }

            var user = await GetUserOrThrow(userId);
            EnsureUserCanCreateGroup(user);

            var family = new FamilyGroupModel
            {
                Name = familyGroupDTO.Name,
                HashCode = GenerateHashCode(),
                Users = new List<string> { userId }
            };

            await _familyGroupCollection.InsertOneAsync(family);
            await UpdateUserAsParent(user, family.Id);

            return family;
        }

        public async Task Join(string hashCode, string userId)
        {
            var familyGroup = await _familyGroupCollection.Find(familyGroup => familyGroup.HashCode == hashCode)
                .FirstOrDefaultAsync();

            if (familyGroup is null)
            {
                throw new Exception("Grupo familiar não encontrado.");
            }

            var user = await GetUserOrThrow(userId);
            EnsureUserCanJoinGroup(user, familyGroup);

            familyGroup.Users.Add(userId);
            await _familyGroupCollection.ReplaceOneAsync(familyGroup => familyGroup.HashCode == hashCode, familyGroup);

            await UpdateUserAsChild(user, familyGroup.Id);
        }

        public async Task<FamilyGroupModel> Update(FamilyGroupModel familyGroup)
        {
            await _familyGroupCollection.ReplaceOneAsync(u => u.Id == familyGroup.Id, familyGroup);
            return familyGroup;
        }

        public async Task<string> SetAdmin(string groupId, string userIdToPromote, string currentUserId)
        {
            var currentUser = await GetUserOrThrow(currentUserId);
            if (currentUser.Role != EnumRoles.Parent)
                throw new UnauthorizedAccessException("Apenas pais podem promover administradores.");

            var familyGroup = await _familyGroupCollection.Find(fg => fg.Id == groupId && fg.Active)
                .FirstOrDefaultAsync();
            if (familyGroup == null || !familyGroup.Users.Contains(userIdToPromote))
                throw new KeyNotFoundException("Usuário ou grupo não encontrado.");

            var userToPromote = await GetUserOrThrow(userIdToPromote);
            if (userToPromote.Role == EnumRoles.Parent)
                throw new InvalidOperationException("O usuário já é administrador.");

            userToPromote.Role = EnumRoles.Parent;
            await _userService.Update(userToPromote);

            return "Usuário promovido a administrador com sucesso!";
        }

        private string GenerateHashCode()
        {
            string str = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string hash = "";
            for (int i = 0; i < 6; i++)
            {
                hash += str[random.Next(str.Length)];
            }

            return '#' + hash;
        }

        private async Task<UserModel> GetUserOrThrow(string userId)
        {
            var user = await _userService.GetUserById(userId);
            return user ?? throw new KeyNotFoundException("Usuário não encontrado.");
        }

        private async Task UpdateUserAsParent(UserModel user, string familyGroupId)
        {
            user.Role = EnumRoles.Parent;
            user.FamilyGroupId = familyGroupId;
            await _userService.Update(user);
        }

        private async Task UpdateUserAsChild(UserModel user, string familyGroupId)
        {
            user.Role = EnumRoles.Child;
            user.FamilyGroupId = familyGroupId;
            await _userService.Update(user);
        }

        private void EnsureUserCanCreateGroup(UserModel user)
        {
            if (!string.IsNullOrEmpty(user.FamilyGroupId))
                throw new InvalidOperationException("O usuário já pertence a um grupo familiar.");
        }

        private void EnsureUserCanJoinGroup(UserModel user, FamilyGroupModel familyGroup)
        {
            if (!string.IsNullOrEmpty(user.FamilyGroupId))
                throw new InvalidOperationException("O usuário já pertence a um grupo familiar.");
            if (familyGroup.Users.Contains(user.Id))
                throw new InvalidOperationException("O usuário já está neste grupo familiar.");
        }
    }
}