using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Services.Medications
{
    /// <summary>
    /// "Banco de dados" em memória.
    ///
    /// Os dados ficam num dicionário enquanto a aplicação estiver rodando e somem quando ela para.
    /// ConcurrentDictionary é usado porque várias requisições podem chegar ao mesmo tempo.
    ///
    /// Esta classe é registrada como Singleton (uma única instância para a aplicação inteira);
    /// se fosse Scoped, cada requisição teria um "banco" vazio.
    /// </summary>
    public class MedicationRepositoryInMemory : IMedicationRepository
    {
        private readonly ConcurrentDictionary<Guid, Medication> _medications = new();

        public Task<IReadOnlyList<Medication>> GetAllAsync()
        {
            IReadOnlyList<Medication> list = _medications.Values.ToList();
            return Task.FromResult(list);
        }

        public Task<Medication?> GetByIdAsync(Guid id)
        {
            _medications.TryGetValue(id, out var medication);
            return Task.FromResult(medication);
        }

        public Task AddAsync(Medication medication)
        {
            _medications[medication.Id] = medication;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Medication medication)
        {
            _medications[medication.Id] = medication;
            return Task.CompletedTask;
        }

        public Task<bool> RemoveAsync(Guid id) =>
            Task.FromResult(_medications.TryRemove(id, out _));
    }
}
