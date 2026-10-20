using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Users;
using CrudKronixForDevs.Core.Resources.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CrudKronixForDevs.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController(
        IAuthApplication authApplication,
        IValidator<LoginRequest> validator) : ControllerBase
    {
        /// <summary>Autentica com e-mail e senha e devolve um token JWT.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var errors = validator.Validate(request);
            if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

            var response = await authApplication.LoginAsync(request);
            if (response is null)
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: AuthMessages.UnauthorizedTitle,
                    detail: AuthMessages.InvalidCredentials);

            return Ok(response);
        }

        /// <summary>Mostra os dados do usuário dono do token. Útil para conferir se o token funciona.</summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Me() => Ok(new
        {
            Id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
            Name = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value,
            Email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        });
    }
}