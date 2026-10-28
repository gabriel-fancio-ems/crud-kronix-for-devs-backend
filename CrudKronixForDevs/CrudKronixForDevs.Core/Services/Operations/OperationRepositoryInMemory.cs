using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using System.Collections.Concurrent;

namespace CrudKronixForDevs.Core.Services.Operations
{
    public class OperationRepositoryInMemory : IOperationRepository
    {
        private readonly ConcurrentDictionary<Guid, Operation> _operations = new();

        public Task<IReadOnlyList<Operation>> GetAllAsync()
        {
            IReadOnlyList<Operation> operations = _operations.Values.ToList();
            return Task.FromResult(operations);
        }

        public Task<Operation?> GetByIdAsync(Guid id)
        {
            _operations.TryGetValue(id, out var operation);
            return Task.FromResult(operation);
        }

        public Task AddAsync(Operation operation)
        {
            _operations[operation.Id] = operation;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Operation operation)
        {
            _operations[operation.Id] = operation;
            return Task.CompletedTask;
        }

        public Task<bool> RemoveAsync(Guid id) =>
            Task.FromResult(_operations.TryRemove(id, out _));
    }
}
