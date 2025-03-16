using Microsoft.AspNetCore.Mvc;
using WebApiMezada.DTOs.Task;
using WebApiMezada.Services.TaskGroup;

namespace WebApiMezada.Controllers
{
    [Route("api/Tasks")]
    [ApiController]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("stats/{familyGroupId}")]
        public async Task<IActionResult> GetTaskStats(string familyGroupId)
        {
            try
            {
                var stats = await _taskService.GetTaskStats(familyGroupId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(string id)
        {
            try
            {
                var task = await _taskService.GetTaskById(id);
                return Ok(task);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? status)
        {
            try
            {
                var tasks = await _taskService.GetAll(status);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskCreateDTO taskDTO)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var task = await _taskService.Create(taskDTO, userId);
                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
          
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> SetAsApproved(string id)
        {
            try
            {
                var parentUserId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(parentUserId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                await _taskService.SetAsApproved(id, parentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> SetAsRejected(string id)
        {
            try
            {
                var parentUserId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(parentUserId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                await _taskService.SetAsRejected(id, parentUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                await _taskService.Delete(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
