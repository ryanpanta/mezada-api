using WebApiMezada.DTOs.User;
using WebApiMezada.Models;

namespace WebApiMezada.Services.User
{
    public interface IUserService
    {
        Task<UserModel> Register(UserRegisterDTO userDTO);
        Task<UserModel> Login(UserLoginDTO userDTO);
        Task<UserModel> GetUserById(string id);
        Task<List<UserModel>> GetAll();
        Task<UserModel> Update(UserModel user);
        Task SetParent(string userId);
    }
}
