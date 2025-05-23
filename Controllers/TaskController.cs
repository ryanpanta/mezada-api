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
        
        [HttpGet("filters/{familyGroupId}")]
        public async Task<IActionResult> GetFilters(string familyGroupId)
        {
            try
            {
                var filters = await _taskService.GetFilters(familyGroupId);
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string filter, [FromQuery] string groupId)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var tasks = await _taskService.GetAll(filter, groupId, userId);
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
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] TaskUpdateDTO taskDTO)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                taskDTO.Id = id;
                var task = await _taskService.Update(taskDTO, userId);
                return Ok(task);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("account-points")]
        public async Task<IActionResult> AccountPoints([FromBody] AccountPointsDTO dto)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                await _taskService.AccountPoints(dto, userId);
                return Ok(new { Message = "Pontos contabilizados com sucesso." });
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
        
        [HttpGet("cycle-summary/{groupId}")]
        public async Task<IActionResult> GetCycleSummary([FromRoute] string groupId)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                var summary = await _taskService.GetCycleSummary(groupId, userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("end-cycle/{groupId}")]
        public async Task<IActionResult> EndCycle([FromRoute] string groupId)
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Message = "Usuário não autenticado." });

                await _taskService.EndCycle(groupId, userId);
                return Ok(new { Message = "Ciclo encerrado e novo ciclo iniciado com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
