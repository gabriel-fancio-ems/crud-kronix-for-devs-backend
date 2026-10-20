using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    /// <summary>
    /// Gera e confere hashes de senha.
    /// O Core só conhece este contrato; a implementação (com a biblioteca do ASP.NET)
    /// fica na pasta Authorization da Api.
    /// </summary>
    public interface IPasswordService
    {
        string Hash(string password);
        bool Verify(string passwordHash, string providedPassword);
    }
}
