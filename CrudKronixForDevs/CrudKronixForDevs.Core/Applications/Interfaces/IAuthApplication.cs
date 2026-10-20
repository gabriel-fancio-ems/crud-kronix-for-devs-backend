using CrudKronixForDevs.Core.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    public interface IAuthApplication
    {
        /// <returns>O token de acesso, ou null se e-mail ou senha estiverem errados.</returns>
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }

}
