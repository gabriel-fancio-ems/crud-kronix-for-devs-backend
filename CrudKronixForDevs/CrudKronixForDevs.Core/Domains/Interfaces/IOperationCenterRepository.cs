namespace CrudKronixForDevs.Core.Domains.Interfaces
{
    /// <summary>
    /// Contrato de armazenamento de centros de operação.
    /// Hoje a implementação guarda tudo em memória (OperationCenterRepositoryInMemory).
    /// </summary>
    public interface IOperationCenterRepository
    {
        Task<IReadOnlyList<OperationCenter>> GetAllAsync();
        Task<OperationCenter?> GetByIdAsync(Guid id);
        Task AddAsync(OperationCenter operationCenter);
        Task UpdateAsync(OperationCenter operationCenter);
        Task<bool> RemoveAsync(Guid id);
    }
}
