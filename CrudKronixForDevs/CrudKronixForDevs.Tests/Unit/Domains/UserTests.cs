using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Domains
{
    public class UserTests
    {
        [Fact]
        public void Create_WithValidData_ShouldFillAllProperties()
        {
            var user = User.Create("Administrador", "admin@kronix.dev", "hash-qualquer");

            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal("Administrador", user.Name);
            Assert.Equal("admin@kronix.dev", user.Email);
            Assert.Equal("hash-qualquer", user.PasswordHash);
        }

        [Fact]
        public void Create_ShouldNormalizeEmailToLowercaseWithoutSpaces()
        {
            var user = User.Create("  Administrador  ", "  Admin@Kronix.DEV  ", "hash");

            Assert.Equal("Administrador", user.Name);
            Assert.Equal("admin@kronix.dev", user.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_WithoutName_ShouldThrowDomainException(string? name)
        {
            var exception = Assert.Throws<DomainException>(() => User.Create(name!, "admin@kronix.dev", "hash"));

            Assert.Equal(AuthMessages.NameRequired, exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("email-sem-arroba")]
        public void Create_WithInvalidEmail_ShouldThrowDomainException(string? email)
        {
            var exception = Assert.Throws<DomainException>(() => User.Create("Admin", email!, "hash"));

            Assert.Equal(AuthMessages.InvalidEmail, exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Create_WithoutPasswordHash_ShouldThrowDomainException(string? passwordHash)
        {
            var exception = Assert.Throws<DomainException>(() => User.Create("Admin", "admin@kronix.dev", passwordHash!));

            Assert.Equal(AuthMessages.PasswordRequired, exception.Message);
        }
    }
}
