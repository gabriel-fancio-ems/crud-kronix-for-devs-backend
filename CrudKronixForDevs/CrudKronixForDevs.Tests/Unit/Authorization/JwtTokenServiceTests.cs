using CrudKronixForDevs.Api.Authorization;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Tests.Fakes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Authorization
{
    /// <summary>
    /// Testa a geração do JWT: conteúdo (claims), validade e assinatura.
    /// </summary>
    public class JwtTokenServiceTests
    {
        private const string Key = "chave-de-teste-com-mais-de-32-caracteres-123456";

        private readonly FixedTimeProvider _clock = new();
        private readonly JwtOptions _options = new()
        {
            Key = Key,
            Issuer = "CrudKronixForDevs",
            Audience = "CrudKronixForDevs",
            ExpirationMinutes = 60
        };

        private readonly User _user = User.Create("Administrador", "admin@kronix.dev", "hash");

        private JwtTokenService CreateService() => new(Options.Create(_options), _clock);

        [Fact]
        public void GenerateToken_ShouldExpireAfterConfiguredMinutes()
        {
            var token = CreateService().GenerateToken(_user);

            Assert.Equal(_clock.GetUtcNow().UtcDateTime.AddMinutes(60), token.ExpiresAt);
        }

        [Fact]
        public void GenerateToken_ShouldContainUserClaims()
        {
            var generated = CreateService().GenerateToken(_user);

            // Lê o token SEM validar, só para conferir o que está dentro dele.
            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(generated.Token);

            Assert.Equal(_user.Id.ToString(), jwt.Subject);
            Assert.Equal("admin@kronix.dev", jwt.GetClaim(JwtRegisteredClaimNames.Email).Value);
            Assert.Equal("Administrador", jwt.GetClaim(JwtRegisteredClaimNames.Name).Value);
            Assert.Equal("CrudKronixForDevs", jwt.Issuer);
            Assert.Contains("CrudKronixForDevs", jwt.Audiences);
        }

        [Fact]
        public void GenerateToken_ShouldNeverContainThePasswordHash()
        {
            var generated = CreateService().GenerateToken(_user);
            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(generated.Token);

            Assert.DoesNotContain(jwt.Claims, c => c.Value == _user.PasswordHash);
        }

        [Fact]
        public void GenerateToken_TwoCalls_ShouldHaveDifferentIds()
        {
            var service = CreateService();
            var handler = new JsonWebTokenHandler();

            var first = handler.ReadJsonWebToken(service.GenerateToken(_user).Token);
            var second = handler.ReadJsonWebToken(service.GenerateToken(_user).Token);

            Assert.NotEqual(first.Id, second.Id); // claim "jti"
        }

        [Fact]
        public async Task GenerateToken_SignedWithKey_ShouldBeValidWithSameKey()
        {
            var generated = CreateService().GenerateToken(_user);

            var result = await new JsonWebTokenHandler().ValidateTokenAsync(generated.Token, ValidationParameters(Key));

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task GenerateToken_ValidatedWithAnotherKey_ShouldBeInvalid()
        {
            // Simula alguém tentando forjar/validar o token sem conhecer a chave secreta.
            var generated = CreateService().GenerateToken(_user);

            var result = await new JsonWebTokenHandler().ValidateTokenAsync(
                generated.Token, ValidationParameters("outra-chave-qualquer-com-mais-de-32-caracteres"));

            Assert.False(result.IsValid);
        }

        // O relógio fixo está em 2026; o validador usa o relógio real. Por isso desligamos a checagem
        // de data aqui: este teste é sobre a ASSINATURA. A expiração é testada nos testes de integração.
        private TokenValidationParameters ValidationParameters(string key) => new()
        {
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = JwtTokenService.CreateSigningKey(key),
            ValidateLifetime = false
        };
    }
}
