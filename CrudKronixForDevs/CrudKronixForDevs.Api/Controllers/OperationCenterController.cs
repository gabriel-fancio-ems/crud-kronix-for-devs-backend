using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Dtos.OperationCenters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Api.Controllers
{
    /// <summary>
    /// CRUD de centros de operação. Segue o mesmo desenho do MedicationController:
    /// recebe a requisição, valida o formato e delega ao Core.
    /// </summary>

    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("api/operation-center")]
    [Produces("application/json")]

    public class OperationCenterController(
            IOperationCenterApplication application,
            IValidator<OperationCenterRequest> validator) : ControllerBase
    {
        /// <summary>Lista os centros de operação. Opcionalmente filtra pela descrição.</summary>
        /// <param name="descricao">Trecho da descrição (sem diferenciar acentos e maiúsculas).</param>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<OperationCenterResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<OperationCenterResponse>>> List([FromQuery] string? descricao)
        {
            var operationCenters = await application.ListAsync(descricao);
            return Ok(operationCenters);
        }

        /// <summary>Busca um centro de operação pelo id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OperationCenterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OperationCenterResponse>> GetById(Guid id)
        {
            var operationCenter = await application.GetByIdAsync(id);
            return operationCenter is null ? NotFound() : Ok(operationCenter);
        }

        /// <summary>Cadastra um novo centro de operação.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(OperationCenterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OperationCenterResponse>> Create([FromBody] OperationCenterRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var created = await application.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Atualiza todos os dados de um centro de operação.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OperationCenterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OperationCenterResponse>> Update(Guid id, [FromBody] OperationCenterRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var updated = await application.UpdateAsync(id, request);
            return updated is null ? NotFound() : Ok(updated);
        }

        /// <summary>Remove um centro de operação. Falha com 409 se houver medicamentos vinculados.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Remove(Guid id)
        {
            var removed = await application.RemoveAsync(id);
            return removed ? NoContent() : NotFound();
        }
    }
}
