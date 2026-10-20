using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Services.Users
{
    public class UserRepositoryInMemory : IUserRepository
    {
        private readonly ConcurrentDictionary<string, CrudKronixForDevs.Core.Domains.User> _users = new(StringComparer.OrdinalIgnoreCase);

        public Task<User?> GetByEmailAsync(string email)
        {
            _users.TryGetValue(email.Trim(), out var user);
            return Task.FromResult(user);
        }

        public Task AddAsync(User user)
        {
            _users[user.Email] = user;
            return Task.CompletedTask;
        }
    }
}
