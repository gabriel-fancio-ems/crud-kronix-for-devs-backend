using CrudKronixForDevs.Core.Applications.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CrudKronixForDevs.Api.Authorization
{
    /// <summary>
    /// Implementa o hash de senha com o PasswordHasher do ASP.NET Core Identity
    /// (PBKDF2 com salt aleatório). Nunca invente seu próprio algoritmo de hash de senha.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        // O tipo genérico só existe para compatibilidade com o Identity; aqui ele não é usado.
        private readonly PasswordHasher<object> _hasher = new();
        private static readonly object UnusedUser = new();

        public string Hash(string password) =>
            _hasher.HashPassword(UnusedUser, password);

        public bool Verify(string passwordHash, string providedPassword) =>
            _hasher.VerifyHashedPassword(UnusedUser, passwordHash, providedPassword)
                != PasswordVerificationResult.Failed;
    }
}