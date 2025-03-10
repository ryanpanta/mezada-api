using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection.Metadata.Ecma335;
using WebApiMezada.Configurations;
using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.Models;
using WebApiMezada.Models.Enums;
using WebApiMezada.Services.User;
using WebApiMezada.Services.User.Validators;

namespace WebApiMezada.Services.FamilyGroup
{
    public class FamilyGroupService : IFamilyGroupService
    {
        private readonly IMongoCollection<FamilyGroupModel> _familyGroupCollection;
        private readonly IUserService _userService;

        public FamilyGroupService(IOptions<FamilyGroupDatabaseSettings> familyGroupsSettings, IUserService userService)
        {
            var client = new MongoClient(familyGroupsSettings.Value.ConnectionString);
            var database = client.GetDatabase(familyGroupsSettings.Value.DatabaseName);
            _familyGroupCollection = database.GetCollection<FamilyGroupModel>(familyGroupsSettings.Value.FamilyGroupCollectionName);
            _userService = userService;
        }

        public async Task<List<FamilyGroupModel>> GetAll()
        {
            return await _familyGroupCollection.Find(familyGroup => true).ToListAsync();
        }

        public async Task<FamilyGroupModel> GetFamilyGroupById(string id)
        {
            var familyGroup = await _familyGroupCollection.Find(familyGroup => familyGroup.Id == id).FirstOrDefaultAsync();
            if (familyGroup is null)
            {
                throw new Exception("Grupo familiar não encontrado.");
            }
            return familyGroup;
        }

        public async Task<FamilyGroupModel> Create(FamilyGroupCreateDTO familyGroupDTO, string userId)
        {
            var user = await _userService.GetUserById(userId);
            if (user is null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            var family = new FamilyGroupModel
            {
                Name = familyGroupDTO.Name,
                HashCode = GenerateHashCode(),
                Users = new List<string> { userId }
            };
            await _familyGroupCollection.InsertOneAsync(family);

            user.Role = EnumRoles.Parent;
            user.FamilyGroupId = family.Id;
            await _userService.Update(user);

            return family;
        }

        public async Task Join(string hashCode, string userId)
        {
            var familyGroup = await _familyGroupCollection.Find(familyGroup => familyGroup.HashCode == hashCode).FirstOrDefaultAsync();
            if (familyGroup is null)
            {
                throw new Exception("Grupo familiar não encontrado.");
            }
            var user = await _userService.GetUserById(userId);
            if (user is null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            familyGroup.Users.Add(userId);
            await _familyGroupCollection.ReplaceOneAsync(familyGroup => familyGroup.HashCode == hashCode, familyGroup);

            user.FamilyGroupId = familyGroup.Id;
            await _userService.Update(user);
        }

        private string GenerateHashCode()
        {
            string str = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string hash = "";
            for(int i = 0; i < 6; i++)
            {
                hash += str[random.Next(str.Length)];
            }
            return '#' + hash;
        }



    }
}
