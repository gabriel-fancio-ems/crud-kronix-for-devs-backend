using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Configuration;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using Microsoft.Extensions.Options;

namespace CrudKronixForDevs.Api.DependencyInjection
{
    /// <summary>
    /// Cria o usuário administrador a partir da seção "User" (User Secrets).
    /// A senha é transformada em hash antes de ser guardada.
    /// </summary>
    public static class WebApplicationExtensions
    {
        public static async Task AdminUserAsync(this WebApplication app)
        {
            // Se a seção estiver incompleta, a linha abaixo lança um erro explicando como configurar.
            var seed = app.Services.GetRequiredService<IOptions<UserOptions>>().Value;

            var passwordService = app.Services.GetRequiredService<IPasswordService>();
            var userRepository = app.Services.GetRequiredService<IUserRepository>();

            var user = User.Create(seed.Name, seed.Email, passwordService.Hash(seed.Password));
            await userRepository.AddAsync(user);

            app.Logger.LogInformation("Usuário carregado: {Email}", user.Email);
        }
    }
}
