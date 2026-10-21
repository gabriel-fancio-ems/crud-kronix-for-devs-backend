using CrudKronixForDevs.Api.Authorization;
using CrudKronixForDevs.Core.Applications;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Users;
using CrudKronixForDevs.Core.Services.Users;
using CrudKronixForDevs.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Applications
{
    /// <summary>
    /// Testes do caso de uso de login.
    ///
    /// Usamos o PasswordService real (hash de verdade) e um FakeTokenService,
    /// porque aqui o foco é a regra "e-mail + senha corretos → token".
    /// </summary>
    public class AuthApplicationTests
    {
        private const string Email = "admin@kronix.dev";
        private const string Password = "Senha@123";

        private readonly UserRepositoryInMemory _userRepository = new();
        private readonly PasswordService _passwordService = new();
        private readonly FakeTokenService _tokenService = new();
        private readonly AuthApplication _application;

        public AuthApplicationTests()
        {
            // Arrange comum a todos os testes: um usuário cadastrado.
            var user = User.Create("Administrador", Email, _passwordService.Hash(Password));
            _userRepository.AddAsync(user).GetAwaiter().GetResult();

            _application = new AuthApplication(_userRepository, _passwordService, _tokenService);
        }

        private Task<LoginResponse?> LoginAsync(string? email, string? password) =>
            _application.LoginAsync(new LoginRequest { Email = email, Password = password });

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_ShouldReturnToken()
        {
            var response = await LoginAsync(Email, Password);

            Assert.NotNull(response);
            Assert.Equal(FakeTokenService.Token, response.AccessToken);
            Assert.Equal("Bearer", response.TokenType);
            Assert.Equal(FakeTokenService.ExpiresAt, response.ExpiresAt);
            Assert.Equal("Administrador", response.Name);
            Assert.Equal(Email, response.Email);
        }

        [Fact]
        public async Task LoginAsync_WithCorrectCredentials_ShouldGenerateTokenForThatUser()
        {
            await LoginAsync(Email, Password);

            Assert.Equal(1, _tokenService.Calls);
            Assert.Equal(Email, _tokenService.LastUser!.Email);
        }

        [Theory]
        [InlineData("ADMIN@KRONIX.DEV")]
        [InlineData("  admin@kronix.dev  ")]
        public async Task LoginAsync_EmailWithDifferentCaseOrSpaces_ShouldSucceed(string email)
        {
            var response = await LoginAsync(email, Password);

            Assert.NotNull(response);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ShouldReturnNullAndNotGenerateToken()
        {
            var response = await LoginAsync(Email, "senha-errada");

            Assert.Null(response);
            Assert.Equal(0, _tokenService.Calls);
        }

        [Fact]
        public async Task LoginAsync_PasswordIsCaseSensitive()
        {
            var response = await LoginAsync(Email, Password.ToLowerInvariant());

            Assert.Null(response);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ShouldReturnNull()
        {
            var response = await LoginAsync("ninguem@kronix.dev", Password);

            Assert.Null(response);
        }

        [Theory]
        [InlineData(null, Password)]
        [InlineData("", Password)]
        [InlineData("   ", Password)]
        [InlineData(Email, null)]
        [InlineData(Email, "")]
        public async Task LoginAsync_WithMissingData_ShouldReturnNull(string? email, string? password)
        {
            var response = await LoginAsync(email, password);

            Assert.Null(response);
            Assert.Equal(0, _tokenService.Calls);
        }
    }
}
