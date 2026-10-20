using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Domains.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
    }
}
