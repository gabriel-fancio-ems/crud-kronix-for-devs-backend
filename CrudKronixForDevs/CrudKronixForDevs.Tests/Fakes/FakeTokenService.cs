using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Fakes
{
    /// <summary>
    /// Implementação "de mentira" do ITokenService.
    ///
    /// Nos testes do AuthApplication queremos testar só a regra de login (e-mail e senha),
    /// e não a geração do JWT. Este fake devolve sempre o mesmo token e guarda para quem
    /// ele foi gerado, para o teste conferir depois.
    ///
    /// Fake escrito à mão = sem biblioteca de mock, sem "mágica" para quem está começando.
    /// </summary>
    public class FakeTokenService : ITokenService
    {
        public const string Token = "fake-token";
        public static readonly DateTime ExpiresAt = new(2026, 1, 15, 11, 0, 0, DateTimeKind.Utc);

        /// <summary>Quantas vezes GenerateToken foi chamado.</summary>
        public int Calls { get; private set; }

        /// <summary>Último usuário para quem o token foi gerado.</summary>
        public User? LastUser { get; private set; }

        public GeneratedToken GenerateToken(User user)
        {
            Calls++;
            LastUser = user;
            return new GeneratedToken(Token, ExpiresAt);
        }
    }

}
