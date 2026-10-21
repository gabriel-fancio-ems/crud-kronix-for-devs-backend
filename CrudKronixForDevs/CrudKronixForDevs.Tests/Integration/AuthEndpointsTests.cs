using CrudKronixForDevs.Core.Dtos.Users;
using CrudKronixForDevs.Core.Resources.Users;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace CrudKronixForDevs.Tests.Integration
{
    /// <summary>
    /// Testes de ponta a ponta do login e da proteção dos endpoints.
    ///
    /// IClassFixture faz a API subir UMA vez para todos os testes desta classe (mais rápido).
    /// </summary>
    public class AuthEndpointsTests(CrudKronixApiFactory factory) : IClassFixture<CrudKronixApiFactory>
    {
        private const string LoginRoute = "/api/auth/login";
        private const string MeRoute = "/api/auth/me";

        private readonly HttpClient _client = factory.CreateClient();

        // ---------------------------------------------------------------- POST /api/auth/login

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
        {
            var response = await _client.PostAsJsonAsync(LoginRoute, new LoginRequest
            {
                Email = CrudKronixApiFactory.TestEmail,
                Password = CrudKronixApiFactory.TestPassword
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(login);
            Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
            Assert.Equal("Bearer", login.TokenType);
            Assert.Equal(CrudKronixApiFactory.TestName, login.Name);
            Assert.Equal(CrudKronixApiFactory.TestEmail, login.Email);
            Assert.True(login.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_EmailInUppercase_ShouldReturn200()
        {
            var response = await _client.PostAsJsonAsync(LoginRoute, new LoginRequest
            {
                Email = CrudKronixApiFactory.TestEmail.ToUpperInvariant(),
                Password = CrudKronixApiFactory.TestPassword
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(CrudKronixApiFactory.TestEmail, "senha-errada")]
        [InlineData("naoexiste@kronix.dev", CrudKronixApiFactory.TestPassword)]
        public async Task Login_WithInvalidCredentials_ShouldReturn401WithSameMessage(string email, string password)
        {
            var response = await _client.PostAsJsonAsync(LoginRoute, new LoginRequest { Email = email, Password = password });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            // A mesma mensagem nos dois casos: quem ataca não descobre se o e-mail existe.
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal(AuthMessages.InvalidCredentials, problem!.Detail);
        }

        [Fact]
        public async Task Login_WithEmptyBody_ShouldReturn400WithFieldErrors()
        {
            var response = await _client.PostAsJsonAsync(LoginRoute, new LoginRequest());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Contains("email", problem!.Errors.Keys);
            Assert.Contains("password", problem.Errors.Keys);
        }

        // ---------------------------------------------------------------- GET /api/auth/me

        [Fact]
        public async Task Me_WithValidToken_ShouldReturnCurrentUser()
        {
            using var client = await factory.CreateAuthenticatedClientAsync();

            var me = await client.GetFromJsonAsync<MeResponse>(MeRoute);

            Assert.NotNull(me);
            Assert.Equal(CrudKronixApiFactory.TestEmail, me.Email);
            Assert.Equal(CrudKronixApiFactory.TestName, me.Name);
            Assert.True(Guid.TryParse(me.Id, out _));
        }

        [Fact]
        public async Task Me_WithoutToken_ShouldReturn401()
        {
            var response = await _client.GetAsync(MeRoute);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ---------------------------------------------------------------- Tokens inválidos

        [Fact]
        public async Task ProtectedEndpoint_WithMalformedToken_ShouldReturn401()
        {
            var response = await SendWithTokenAsync("isto.nao.e.um.token");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithTamperedToken_ShouldReturn401()
        {
            // Um JWT tem 3 partes: cabeçalho.conteúdo.assinatura
            // Alteramos o PRIMEIRO caractere da assinatura (o último pode ser só preenchimento do base64).
            var token = await factory.GetTokenAsync();
            var parts = token.Split('.');
            var signature = parts[2];
            parts[2] = (signature[0] == 'A' ? 'B' : 'A') + signature[1..];
            var tampered = string.Join('.', parts);

            var response = await SendWithTokenAsync(tampered);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithExpiredToken_ShouldReturn401()
        {
            var response = await SendWithTokenAsync(factory.CreateExpiredToken());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private Task<HttpResponseMessage> SendWithTokenAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, MeRoute);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _client.SendAsync(request);
        }

        // Formato do JSON devolvido por /api/auth/me.
        private record MeResponse(string Id, string Name, string Email);
    }
}