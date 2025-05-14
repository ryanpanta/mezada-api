using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.Suggestion;
using WebApiMezada.Services.FamilyGroup;

namespace WebApiMezada.Controllers
{
    [Route("api/Suggestion")]
    [ApiController]
    public class SuggestionController : Controller
    {
        private readonly ISuggestionService _suggestionService;

        public SuggestionController(ISuggestionService suggestionService)
        {
            _suggestionService = suggestionService;
        }

        [HttpGet]
        //[RequireAuthentication]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var suggestions = await _suggestionService.GetSuggestions(userId);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateSuggestion([FromBody] SuggestionCreateDTO dto)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var suggestion = await _suggestionService.CreateOrUpdateSuggestion(dto, userId);
                return CreatedAtAction(nameof(GetAll), new { userId }, suggestion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}