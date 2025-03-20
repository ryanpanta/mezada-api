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
            var client = new MongoClient(userSettings.Value.ConnectionString);
            var database = client.GetDatabase(userSettings.Value.DatabaseName);
            _userCollection = database.GetCollection<UserModel>(userSettings.Value.UserCollectionName);
            _validator = validator;
        }

        public async Task<List<UserModel>> GetAll()
        {
            return await _userCollection.Find(user => true).ToListAsync();
        }

        public async Task<UserModel> Update(UserModel user)
        {
            await _userCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
            return user;
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
                Color = GenerateColorAndBackGround().Color,
                BackgroundColor = GenerateColorAndBackGround().BackgroundColor
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

        public async Task SetParent(string userId)
        {
            var user = await GetUserById(userId);
            user.SetParent();
            await Update(user);
        }

        private ColorScheme GenerateColorAndBackGround()
        {
            var colors = new string[]
             {
                "#FFFFFF", // Branco para texto
                "#F0F4C3", // Verde limão claro
                "#B3E5FC", // Azul claro
                "#FFCCBC", // Coral claro
                "#D1C4E9", // Roxo claro
                "#FFECB3"  // Amarelo claro
             };
     
            var backgroundColors = new string[]
            {
                "#1976D2", // Azul escuro
                "#388E3C", // Verde escuro
                "#0288D1", // Azul médio
                "#D81B60", // Rosa escuro
                "#7B1FA2", // Roxo escuro
                "#F57C00"  // Laranja escuro
            };

            var random = new Random().Next(0, colors.Length);

            var color = colors[random];
            var backgroundColor = backgroundColors[random];

            return new ColorScheme { 
                Color = color, 
                BackgroundColor = backgroundColor
            };
        }

    }
}
