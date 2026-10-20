using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Dtos.Medications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Api.Controllers
{
    /// <summary>
    /// Controller = porta de entrada HTTP.
    /// Ele só faz três coisas: recebe a requisição, valida o formato e delega ao Core.
    /// Nenhuma regra de negócio mora aqui.
    ///
    /// [Authorize] exige um token válido em TODOS os endpoints deste controller.
    /// Sem token, a API responde 401 antes mesmo de chegar aqui.
    /// </summary>

    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("api/medication")]
    [Produces("application/json")]

    public class MedicationController(
            IMedicationApplication application,
            IValidator<MedicationRequest> validator) : ControllerBase
    {
        /// <summary>Lista os medicamentos. Opcionalmente filtra por nome ou princípio ativo.</summary>
        /// <param name="nome">Trecho do nome ou do princípio ativo (sem diferenciar acentos e maiúsculas).</param>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<MedicationResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MedicationResponse>>> List([FromQuery] string? nome)
        {
            var medications = await application.ListAsync(nome);
            return Ok(medications);
        }

        /// <summary>Busca um medicamento pelo id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MedicationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicationResponse>> GetById(Guid id)
        {
            var medication = await application.GetByIdAsync(id);
            return medication is null ? NotFound() : Ok(medication);
        }

        /// <summary>Cadastra um novo medicamento.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(MedicationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MedicationResponse>> Create([FromBody] MedicationRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var created = await application.CreateAsync(request);

            // 201 Created + cabeçalho Location apontando para GET /api/medicamentos/{id}
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Atualiza todos os dados de um medicamento.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MedicationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MedicationResponse>> Update(Guid id, [FromBody] MedicationRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var updated = await application.UpdateAsync(id, request);
            return updated is null ? NotFound() : Ok(updated);
        }

        /// <summary>Remove um medicamento.</summary>
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
