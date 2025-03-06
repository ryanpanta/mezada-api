using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult> Create(UserModel user)
        {
            await _userService.Create(user);
            return NoContent();
        }
    }
}
