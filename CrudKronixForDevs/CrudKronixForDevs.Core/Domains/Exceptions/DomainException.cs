using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Domains.Exceptions
{
    /// <summary>
    /// Lançada quando uma regra de negócio é violada.
    /// O middleware da Api converte essa exceção em uma resposta HTTP 400.
    /// </summary>
    public class DomainException(string message) : Exception(message)
    {
    }

    /// <summary>
    /// Lançada quando já existe um medicamento igual cadastrado.
    /// O middleware da Api converte essa exceção em uma resposta HTTP 409 (Conflict).
    /// </summary>
    public class DuplicateMedicationException(string message) : DomainException(message)
    {
    }
}
