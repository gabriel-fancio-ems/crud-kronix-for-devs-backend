using CrudKronixForDevs.Api.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Authorization
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _service = new();

        [Fact]
        public void Hash_ShouldNotContainThePlainTextPassword()
        {
            var hash = _service.Hash("A123456@");

            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.DoesNotContain("A123456@", hash);
        }

        [Fact]
        public void Hash_SamePasswordTwice_ShouldGenerateDifferentHashes()
        {
            // Cada hash usa um "salt" aleatório. Duas pessoas com a mesma senha têm hashes diferentes.
            Assert.NotEqual(_service.Hash("A123456@"), _service.Hash("A123456@"));
        }

        [Fact]
        public void Verify_WithCorrectPassword_ShouldReturnTrue()
        {
            var hash = _service.Hash("A123456@");

            Assert.True(_service.Verify(hash, "A123456@"));
        }

        [Theory]
        [InlineData("a123456@")]   // maiúscula/minúscula importa
        [InlineData("A123456")]    // faltando um caractere
        [InlineData("")]
        public void Verify_WithWrongPassword_ShouldReturnFalse(string wrongPassword)
        {
            var hash = _service.Hash("A123456@");

            Assert.False(_service.Verify(hash, wrongPassword));
        }
    }
}
