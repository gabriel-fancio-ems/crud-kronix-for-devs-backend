using CrudKronixForDevs.Core.Dtos.Operations;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    public interface IOperationApplication
    {
        Task<IReadOnlyList<OperationResponse>> ListAsync(string? operation);
        Task<OperationResponse?> GetByIdAsync(Guid id);
        Task<OperationResponse> CreateAsync(OperationRequest request);
        Task<OperationResponse?> UpdateAsync(Guid id, OperationRequest request);
        Task<bool> RemoveAsync(Guid id);
    }
}
