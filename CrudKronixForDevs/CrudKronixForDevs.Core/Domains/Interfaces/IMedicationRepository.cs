using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Domains.Interfaces
{
    /// <summary>
    /// Contrato de armazenamento de medicamentos.
    ///
    /// A aplicação depende desta INTERFACE, e não de uma implementação concreta.
    /// Hoje a implementação guarda tudo em memória (MedicationRepositoryInMemory).
    ///
    /// Os métodos são assíncronos (Task) mesmo em memória, porque um banco real seria assíncrono.
    /// </summary>
    public interface IMedicationRepository
    {
        Task<IReadOnlyList<Medication>> GetAllAsync();
        Task<Medication?> GetByIdAsync(Guid id);
        Task AddAsync(Medication medication);
        Task UpdateAsync(Medication medication);
        Task<bool> RemoveAsync(Guid id);
    }
}
