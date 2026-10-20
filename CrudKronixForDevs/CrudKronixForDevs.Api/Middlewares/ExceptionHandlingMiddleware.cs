using CrudKronixForDevs.Core.Domains.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Api.Middlewares
{
    /// <summary>
    /// Middleware = um "filtro" por onde toda requisição passa.
    ///
    /// Este aqui envolve o resto da pipeline num try/catch e transforma exceções em
    /// respostas HTTP padronizadas (ProblemDetails, RFC 9457). Assim os controllers
    /// não precisam ter try/catch espalhado.
    /// </summary>
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (DuplicateMedicationException ex)
            {
                logger.LogWarning("Conflito: {Mensagem}", ex.Message);
                await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Medicamento duplicado", ex.Message);
            }
            catch (DomainException ex)
            {
                logger.LogWarning("Regra de negócio violada: {Mensagem}", ex.Message);
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Regra de negócio violada", ex.Message);
            }
            catch (Exception ex)
            {
                // Erros inesperados: registramos tudo no log, mas não expomos detalhes ao cliente.
                logger.LogError(ex, "Erro não tratado ao processar {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
                await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Erro interno",
                    "Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }

        private static Task WriteProblemAsync(HttpContext context, int status, string titulo, string detalhe)
        {
            var problem = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            return context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }
}