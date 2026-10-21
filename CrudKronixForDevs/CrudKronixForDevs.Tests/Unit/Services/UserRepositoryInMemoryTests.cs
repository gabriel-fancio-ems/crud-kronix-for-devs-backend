using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Services.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Services
{
    public class UserRepositoryInMemoryTests
    {
        private readonly UserRepositoryInMemory _repository = new();

        [Fact]
        public async Task GetByEmailAsync_AfterAdd_ShouldReturnUser()
        {
            var user = User.Create("Admin", "admin@kronix.dev", "hash");
            await _repository.AddAsync(user);

            var found = await _repository.GetByEmailAsync("admin@kronix.dev");

            Assert.Same(user, found);
        }

        [Theory]
        [InlineData("ADMIN@KRONIX.DEV")]
        [InlineData("  admin@kronix.dev  ")]
        public async Task GetByEmailAsync_ShouldIgnoreCaseAndSpaces(string email)
        {
            await _repository.AddAsync(User.Create("Admin", "admin@kronix.dev", "hash"));

            Assert.NotNull(await _repository.GetByEmailAsync(email));
        }

        [Fact]
        public async Task GetByEmailAsync_UnknownEmail_ShouldReturnNull()
        {
            Assert.Null(await _repository.GetByEmailAsync("ninguem@kronix.dev"));
        }

        [Fact]
        public async Task AddAsync_SameEmailTwice_ShouldKeepTheLastUser()
        {
            await _repository.AddAsync(User.Create("Primeiro", "admin@kronix.dev", "hash"));
            await _repository.AddAsync(User.Create("Segundo", "admin@kronix.dev", "hash"));

            var found = await _repository.GetByEmailAsync("admin@kronix.dev");

            Assert.Equal("Segundo", found!.Name);
        }
    }
}
