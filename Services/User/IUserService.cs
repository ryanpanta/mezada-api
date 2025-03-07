using WebApiMezada.DTOs.User;
using WebApiMezada.Models;

namespace WebApiMezada.Services.User
{
    public interface IUserService
    {
        Task<UserModel> Register(UserRegisterDTO userDTO);
    }
}
