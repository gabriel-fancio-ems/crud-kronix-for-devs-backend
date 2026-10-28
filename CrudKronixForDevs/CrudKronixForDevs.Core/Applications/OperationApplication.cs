using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using CrudKronixForDevs.Core.Dtos.Operations;
using CrudKronixForDevs.Core.Extensions;

namespace CrudKronixForDevs.Core.Applications
{
    public class OperationApplication(IOperationRepository repository, TimeProvider timeProvider) : IOperationApplication
    {
        public async Task<IReadOnlyList<OperationResponse>> ListAsync(string? operation)
        {
            var operations = await repository.GetAllAsync();
            var filter = operation.NormalizeForSearch();

            return operations
                .Where(o => filter.Length == 0 || o.Name.NormalizeForSearch().Contains(filter))
                .OrderBy(o => o.Name)
                .Select(o => o.ToResponse())
                .ToList();
        }

        public async Task<OperationResponse?> GetByIdAsync(Guid id)
        {
            var operation = await repository.GetByIdAsync(id);
            return operation?.ToResponse();
        }

        public async Task<OperationResponse> CreateAsync(OperationRequest request)
        {
            var operation = Operation.Add(
                request.ProductionOrderId ?? Guid.Empty,
                request.Operation ?? string.Empty,
                request.WorkCenterId ?? Guid.Empty,
                request.Status ?? default,
                timeProvider.GetLocalNow().DateTime);

            await repository.AddAsync(operation);
            return operation.ToResponse();
        }

        public async Task<OperationResponse?> UpdateAsync(Guid id, OperationRequest request)
        {
            var operation = await repository.GetByIdAsync(id);
            if (operation is null) return null;

            operation.Update(
                request.ProductionOrderId ?? Guid.Empty,
                request.Operation ?? string.Empty,
                request.WorkCenterId ?? Guid.Empty,
                request.Status ?? default);

            await repository.UpdateAsync(operation);
            return operation.ToResponse();
        }

        public Task<bool> RemoveAsync(Guid id) => repository.RemoveAsync(id);
    }
}
