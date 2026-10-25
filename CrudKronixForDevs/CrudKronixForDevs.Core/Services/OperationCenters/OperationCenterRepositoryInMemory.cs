using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using System.Collections.Concurrent;

namespace CrudKronixForDevs.Core.Services.OperationCenters
{
    /// <summary>
    /// "Banco de dados" em memória de centros de operação.
    /// Registrado como Singleton, pelo mesmo motivo do MedicationRepositoryInMemory.
    /// </summary>
    public class OperationCenterRepositoryInMemory : IOperationCenterRepository
    {
        private readonly ConcurrentDictionary<Guid, OperationCenter> _operationCenters = new();

        public Task<IReadOnlyList<OperationCenter>> GetAllAsync()
        {
            IReadOnlyList<OperationCenter> list = _operationCenters.Values.ToList();
            return Task.FromResult(list);
        }

        public Task<OperationCenter?> GetByIdAsync(Guid id)
        {
            _operationCenters.TryGetValue(id, out var operationCenter);
            return Task.FromResult(operationCenter);
        }

        public Task AddAsync(OperationCenter operationCenter)
        {
            _operationCenters[operationCenter.Id] = operationCenter;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(OperationCenter operationCenter)
        {
            _operationCenters[operationCenter.Id] = operationCenter;
            return Task.CompletedTask;
        }

        public Task<bool> RemoveAsync(Guid id) =>
            Task.FromResult(_operationCenters.TryRemove(id, out _));
    }
}
