using CrudKronixForDevs.Core.Dtos.OperationCenters;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    /// <summary>
    /// Casos de uso de centros de operação.
    /// O Controller conversa apenas com esta interface.
    /// </summary>
    public interface IOperationCenterApplication
    {
        Task<IReadOnlyList<OperationCenterResponse>> ListAsync(string? description);
        Task<OperationCenterResponse?> GetByIdAsync(Guid id);
        Task<OperationCenterResponse> CreateAsync(OperationCenterRequest request);

        /// <returns>O centro de operação atualizado, ou null se o id não existir.</returns>
        Task<OperationCenterResponse?> UpdateAsync(Guid id, OperationCenterRequest request);

        /// <returns>true se removeu; false se o id não existir.</returns>
        Task<bool> RemoveAsync(Guid id);
    }
}
