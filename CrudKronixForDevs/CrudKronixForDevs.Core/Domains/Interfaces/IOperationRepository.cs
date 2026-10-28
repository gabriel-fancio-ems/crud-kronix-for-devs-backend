namespace CrudKronixForDevs.Core.Domains.Interfaces
{
    public interface IOperationRepository
    {
        Task<IReadOnlyList<Operation>> GetAllAsync();
        Task<Operation?> GetByIdAsync(Guid id);
        Task AddAsync(Operation operation);
        Task UpdateAsync(Operation operation);
        Task<bool> RemoveAsync(Guid id);
    }
}
