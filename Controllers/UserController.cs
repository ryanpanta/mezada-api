using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.User;
using WebApiMezada.Middleware.Attributes;
using WebApiMezada.Models;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.User;

namespace WebApiMezada.Controllers
{
    [Route("api/Users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IFamilyGroupService _familyGroupService;
        public UserController(IUserService userService, IFamilyGroupService familyGroupService)
        {
            _userService = userService;
            _familyGroupService = familyGroupService;
        }

        [HttpGet]
        //[RequireAuthentication]
        public async Task<ActionResult<List<UserModel>>> GetAll()
        {
            var response = await _userService.GetAll();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                return Ok(new {Name = user.Name, Color = user.Color, BackgroundColor = user.BackgroundColor});
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPut("/SetAsParent")]
        public async Task<ActionResult> SetAsParent([FromBody] string id)
        {
            try
            {
                await _userService.SetAsParent(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet("/FamilyGroup/{id}")]
        public async Task<ActionResult<UserModel>> GetUsersByFamilyGroup(string id)
        {
            try
            {
                var users = await _userService.GetUsersByFamilyGroup(id);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
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

                string familyGroupName = string.Empty;
                if (!string.IsNullOrEmpty(user.FamilyGroupId))
                {
                    var familyGroup = await _familyGroupService.GetFamilyGroupById(user.FamilyGroupId);
                    familyGroupName = familyGroup?.Name ?? string.Empty;
                }

                return Ok(new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    FamilyGroupId = user.FamilyGroupId,
                    FamilyGroupName = familyGroupName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
