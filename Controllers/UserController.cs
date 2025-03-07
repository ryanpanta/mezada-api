using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.User;
using WebApiMezada.Models;
using WebApiMezada.Services.User;

namespace WebApiMezada.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserModel>>> GetAll()
        {
            var response = await _userService.GetAll();
            return Ok(response);
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] UserRegisterDTO userDTO)
        {
            try
            {
                var user = await _userService.Register(userDTO);
                return Ok(new { UserId = user.Id, Message = "Usuário cadastrado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDTO userDTO)
        {
            try
            {
                var user = await _userService.Login(userDTO);
                return Ok(new { UserId = user.Id, Message = "Login bem-sucedido" });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }
    }
}
