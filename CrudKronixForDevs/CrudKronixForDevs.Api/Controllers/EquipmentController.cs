using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Dtos.Equipments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Api.Controllers
{
    /// <summary>
    /// Cadastro e listagem de equipamentos. Segue o mesmo desenho do OperationCenterController:
    /// recebe a requisição, valida o formato e delega ao Core.
    /// </summary>

    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("api/equipment")]
    [Produces("application/json")]

    public class EquipmentController(
            IEquipmentApplication application,
            IValidator<EquipmentRequest> validator) : ControllerBase
    {
        /// <summary>Lista todos os equipamentos.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<EquipmentResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> List()
        {
            var equipments = await application.ListAsync();
            return Ok(equipments);
        }

        /// <summary>Cadastra um novo equipamento. Falha com 409 se o código já existir.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EquipmentResponse>> Create([FromBody] EquipmentRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var created = await application.CreateAsync(request);

            // Ainda não há GetById, então devolve 201 sem o header Location.
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>Busca um equipamento pelo id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EquipmentResponse>> GetById(Guid id)
        {
            var equipment = await application.GetByIdAsync(id);
            return equipment is null ? NotFound() : Ok(equipment);
        }

        /// <summary>Atualiza todos os dados de um equipamento.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EquipmentResponse>> Update(Guid id, [FromBody] EquipmentRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var updated = await application.UpdateAsync(id, request);
            return updated is null ? NotFound() : Ok(updated);
        }
    }
}
