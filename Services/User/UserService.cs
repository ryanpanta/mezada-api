using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.Models;

namespace WebApiMezada.Services.User
{
    public class UserService
    {
        private readonly IMongoCollection<UserModel> _userCollection;

        public UserService(IOptions<UserDatabaseSettings> userSettings)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Mezada");
            _userCollection = database.GetCollection<UserModel>("Users");
        }

        public async Task<List<UserModel>> GetAll()
        {
            return await _userCollection.Find(user => true).ToListAsync();
        }

        public async Task<UserModel> Get(string id)
        {
            return await _userCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task Create(UserModel user)
        {
            await _userCollection.InsertOneAsync(user);
        }

    }
}
