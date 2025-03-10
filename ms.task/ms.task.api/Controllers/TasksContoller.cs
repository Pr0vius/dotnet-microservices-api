using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ms.task.application.Commands;
using ms.task.application.Queries;
using ms.task.application.Request;
using System.Security.Claims;

namespace ms.task.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksContoller : ControllerBase
    {
        private readonly IMediator _mediator;

        public TasksContoller(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();

            return Ok(await _mediator.Send(new GetAllTasksQuery(userId)));
        }

        [HttpGet]
        [Authorize]
        [Route("/{TaskId}")]
        public async Task<IActionResult> GetById(Guid TaskId)
        {
            var userId = GetUserId();

            return Ok(await _mediator.Send(new GetTaskByIdQuery(userId, TaskId)));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest taskRequest)
        {
            var userId = GetUserId();
            return Ok(await _mediator.Send(new CreateTaskCommand(userId, taskRequest.Name, taskRequest.Description)));

        }

        [HttpPut]
        [Authorize]
        [Route("/{TaskId}")]
        public async Task<IActionResult> UpdateTask(Guid TaskId)
        {
            var userId = GetUserId();
            return Ok(await _mediator.Send(new ChangeTaskStateCommand(userId, TaskId)));

        }

        [HttpDelete]
        [Authorize]
        [Route("/{TaskId}")]
        public async Task<IActionResult> DeleteTask(Guid TaskId)
        {
            var userId = GetUserId();
            return Ok(await _mediator.Send(new DeleteTaskCommand(userId, TaskId)));
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");


            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario.");
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new Exception("El ID del usuario no es un GUID válido.");
            }

            return userId;
        }
    }
}
