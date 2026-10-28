using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Dtos.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Api.Controllers
{
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("api/operation")]
    [Produces("application/json")]
    public class OperationController(
        IOperationApplication application,
        IValidator<OperationRequest> validator) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<OperationResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<OperationResponse>>> List([FromQuery] string? operation)
        {
            var operations = await application.ListAsync(operation);
            return Ok(operations);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OperationResponse>> GetById(Guid id)
        {
            var operation = await application.GetByIdAsync(id);
            return operation is null ? NotFound() : Ok(operation);
        }

        [HttpPost]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OperationResponse>> Create([FromBody] OperationRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var created = await application.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OperationResponse>> Update(Guid id, [FromBody] OperationRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var updated = await application.UpdateAsync(id, request);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(Guid id)
        {
            var removed = await application.RemoveAsync(id);
            return removed ? NoContent() : NotFound();
        }
    }
}
