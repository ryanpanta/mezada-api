using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApiMezada.Configurations;
using WebApiMezada.Models;
using BCrypt.Net;
using WebApiMezada.DTOs.User;
using WebApiMezada.Services.User.Validators;

namespace WebApiMezada.Services.User
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly UserRegisterValidator _validator;

        public UserService(IOptions<UserDatabaseSettings> userSettings, UserRegisterValidator validator)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Mezada");
            _userCollection = database.GetCollection<UserModel>("Users");
            _validator = validator;
        }

        public async Task<List<UserModel>> GetAll()
        {
            return await _userCollection.Find(user => true).ToListAsync();
        }

        public async Task<UserModel> GetUserById(string id)
        {
            var user = await _userCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
            if (user is null)
            {
                throw new Exception("Usuário não encontrado.");
            }
            return user;
        }


        public async Task<UserModel> Register(UserRegisterDTO userDTO)
        {
            
            var existingUser = await _userCollection.Find(u => u.Email == userDTO.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new Exception("Email já cadastrado.");
            }

            var validationResult = _validator.Validate(userDTO);

            if (!validationResult.IsValid)
            {
                throw new Exception(validationResult.Errors.First().ErrorMessage);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

            var user = new UserModel
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Password = passwordHash,
            };

            await _userCollection.InsertOneAsync(user);
            return user;
        }

        public async Task<UserModel> Login(UserLoginDTO userDTO)
        {
            var user = await _userCollection.Find(u => u.Email == userDTO.Email).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(userDTO.Password, user.Password))
            {
                throw new Exception("Email ou senha inválidos.");
            }

            return user; 
        }

    }
}
