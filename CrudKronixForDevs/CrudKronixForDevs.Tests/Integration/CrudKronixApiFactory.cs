using System.Net.Http.Headers;
using System.Net.Http.Json;
using CrudKronixForDevs.Api.Authorization;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Users;
using CrudKronixForDevs.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrudKronixForDevs.Tests.Integration
{
    /// <summary>
    /// Sobe a API DE VERDADE em memória, sem abrir porta, para testar de ponta a ponta:
    /// rota → middleware → autenticação → controller → validação → Core → resposta HTTP.
    ///
    /// Os testes NÃO dependem do User Secrets da sua máquina. Aqui definimos um usuário
    /// e uma chave JWT só para teste, que sobrescrevem qualquer outro valor de configuração.
    /// Assim os testes rodam igual no seu PC, no do colega e no pipeline de CI.
    /// </summary>
    public class CrudKronixApiFactory : WebApplicationFactory<Program>
    {
        public const string TestEmail = "teste@kronix.dev";
        public const string TestName = "Usuário de Teste";
        public const string TestPassword = "SenhaDeTeste@123";
        public const string TestJwtKey = "chave-de-teste-com-mais-de-32-caracteres-123456";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["User:Email"] = TestEmail,
                    ["User:Name"] = TestName,
                    ["User:Password"] = TestPassword,
                    ["Jwt:Key"] = TestJwtKey
                });
            });
        }

        /// <summary>Faz login com o usuário de teste e devolve o token.</summary>
        public async Task<string> GetTokenAsync()
        {
            using var client = CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest { Email = TestEmail, Password = TestPassword });
            response.EnsureSuccessStatusCode();

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return login!.AccessToken;
        }

        /// <summary>Cria um HttpClient que já envia "Authorization: Bearer {token}" em toda requisição.</summary>
        public async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
            return client;
        }

        /// <summary>
        /// Gera um token com a mesma chave da API, mas emitido no passado (já expirado).
        /// Usado para provar que a API recusa tokens vencidos.
        /// </summary>
        public string CreateExpiredToken()
        {
            var jwtOptions = Services.GetRequiredService<IOptions<JwtOptions>>();
            var twoHoursAgo = new FixedTimeProvider(DateTimeOffset.UtcNow.AddHours(-2));
            var service = new JwtTokenService(jwtOptions, twoHoursAgo);

            var user = User.Create(TestName, TestEmail, "hash");
            return service.GenerateToken(user).Token;
        }
    }
}
