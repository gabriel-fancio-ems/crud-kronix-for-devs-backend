using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    /// <summary>
    /// Gera o token de acesso de um usuário autenticado.
    /// A implementação com JWT fica na pasta Authorization da Api.
    /// </summary>
    public interface ITokenService
    {
        GeneratedToken GenerateToken(User user);
    }

}
