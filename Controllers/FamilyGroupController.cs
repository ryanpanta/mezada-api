using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.FamilyGroup;
using WebApiMezada.DTOs.User;
using WebApiMezada.Middleware.Attributes;
using WebApiMezada.Models;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.User;

namespace WebApiMezada.Controllers
{
    [Route("api/FamilyGroups")]
    [ApiController]
    public class FamilyGroupController : Controller
    {
        private readonly IFamilyGroupService _familyGroupService;
        public FamilyGroupController(IFamilyGroupService familyGroupService)
        {
            _familyGroupService = familyGroupService;
        }


        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<FamilyGroupModel>> GetFamilyGroupById([FromRoute] string id)
        {
            try { 
                var response = await _familyGroupService.GetFamilyGroupById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> Create([FromBody] FamilyGroupCreateDTO familyGroupDTO)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });
                var familyGroup = await _familyGroupService.Create(familyGroupDTO, userId);
                return Ok(new { Message = "Grupo familiar criado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

        }

        [HttpPost("join")]
        public async Task<ActionResult> Join([FromBody] FamilyGroupJoinDTO hashCode)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });
                await _familyGroupService.Join(hashCode.HashCode, userId);

                return Ok(new { Message = "Parabéns, você entrou no grupo familiar." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
