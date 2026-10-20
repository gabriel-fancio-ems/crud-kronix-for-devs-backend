using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Users;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CrudKronixForDevs.Api.Authorization
{
    /// <summary>
    /// Gera o JWT: um texto assinado com a chave secreta que carrega quem é o usuário (claims)
    /// e até quando vale. A API confere a assinatura a cada requisição; se alguém alterar o token,
    /// a assinatura deixa de bater e o acesso é negado.
    /// </summary>
    public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
    {
        public GeneratedToken GenerateToken(User user)
        {
            var jwt = options.Value;
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var expiresAt = now.AddMinutes(jwt.ExpirationMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = jwt.Issuer,
                Audience = jwt.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(CreateSigningKey(jwt.Key), SecurityAlgorithms.HmacSha256)
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);
            return new GeneratedToken(token, expiresAt);
        }

        /// <summary>Converte a chave de texto na chave usada para assinar e validar o token.</summary>
        public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
    }
}