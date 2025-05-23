using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.Task;
using WebApiMezada.DTOs.User;
using WebApiMezada.Middleware.Attributes;
using WebApiMezada.Models;
using WebApiMezada.Services.FamilyGroup;
using WebApiMezada.Services.TaskGroup;
using WebApiMezada.Services.User;

namespace WebApiMezada.Controllers
{
    [Route("api/TaskHistory")]
    [ApiController]
    public class TaskHistoryController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskHistoryController(ITaskService taskService)
        {
            _taskService = taskService;
        }
        
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskHistory([FromRoute] string taskId)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var history = await _taskService.GetTaskHistory(taskId, userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("revert")]
        public async Task<IActionResult> RevertHistory([FromBody] RevertHistoryDTO dto)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                await _taskService.RevertHistory(dto, userId);
                return Ok(new { Message = "Contabilização revertida com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
