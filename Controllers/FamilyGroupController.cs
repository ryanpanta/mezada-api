using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.User;
using WebApiMezada.Middleware.Attributes;
using WebApiMezada.Models;
using WebApiMezada.Services.User;

namespace WebApiMezada.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [RequireAuthentication]
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

        [HttpGet("me")]
        [RequireAuthentication]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var user = await _userService.GetUserById(userId);
                if (user == null)
                    return NotFound(new { Message = "Usuário não encontrado." });

                return Ok(new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
