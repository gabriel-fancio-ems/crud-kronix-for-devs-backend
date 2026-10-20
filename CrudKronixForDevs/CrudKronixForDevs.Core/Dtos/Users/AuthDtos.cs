using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Dtos.Users
{
    /// <summary>JSON enviado em POST /api/auth/login.</summary>
    public class LoginRequest
    {
        /// <example>admin@exemplo.com</example>
        public string? Email { get; init; }

        public string? Password { get; init; }
    }

    /// <summary>JSON devolvido quando o login dá certo.</summary>
    public record LoginResponse(
        string AccessToken,
        string TokenType,
        DateTime ExpiresAt,
        string Name,
        string Email);

    /// <summary>Resultado interno da geração do token (data de expiração em UTC).</summary>
    public record GeneratedToken(string Token, DateTime ExpiresAt);

}
