using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains.Interfaces;
using CrudKronixForDevs.Core.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Applications
{
    /// <summary>
    /// Caso de uso de login: confere e-mail e senha e, se estiverem certos, devolve um token.
    /// </summary>
    public class AuthApplication(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenService tokenService) : IAuthApplication
    {
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
                return null;

            var user = await userRepository.GetByEmailAsync(request.Email);

            // A mesma resposta para "e-mail não existe" e "senha errada".
            // Assim, quem tenta adivinhar não descobre quais e-mails estão cadastrados.
            if (user is null || !passwordService.Verify(user.PasswordHash, request.Password))
                return null;

            var token = tokenService.GenerateToken(user);
            return new LoginResponse(token.Token, "Bearer", token.ExpiresAt, user.Name, user.Email);
        }
    }
}
