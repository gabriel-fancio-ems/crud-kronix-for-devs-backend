using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Dtos.Users;
using CrudKronixForDevs.Core.Resources.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Validation
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldReturnNoErrors()
        {
            var errors = _validator.Validate(new LoginRequest { Email = "admin@kronix.dev", Password = "123" });

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_EmptyRequest_ShouldReturnEmailAndPasswordErrors()
        {
            var errors = _validator.Validate(new LoginRequest());

            Assert.Equal(AuthMessages.InvalidEmail, Assert.Single(errors["email"]));
            Assert.Equal(AuthMessages.PasswordRequired, Assert.Single(errors["password"]));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("email-sem-arroba")]
        public void Validate_InvalidEmail_ShouldReturnEmailError(string? email)
        {
            var errors = _validator.Validate(new LoginRequest { Email = email, Password = "123" });

            Assert.True(errors.ContainsKey("email"));
            Assert.False(errors.ContainsKey("password"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Validate_WithoutPassword_ShouldReturnPasswordError(string? password)
        {
            var errors = _validator.Validate(new LoginRequest { Email = "admin@kronix.dev", Password = password });

            Assert.True(errors.ContainsKey("password"));
        }
    }
}
