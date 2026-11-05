namespace CrudKronixForDevs.Core.Domains.Interfaces
{
    /// <summary>
    /// Contrato de armazenamento de equipamentos.
    /// Hoje a implementação guarda tudo em memória (EquipmentRepositoryInMemory).
    /// </summary>
    public interface IEquipmentRepository
    {
        Task<IReadOnlyList<Equipment>> GetAllAsync();
        Task AddAsync(Equipment equipment);
        Task<Equipment?> GetByIdAsync(Guid id);
        Task UpdateAsync(Equipment equipment);
        Task<bool> RemoveAsync(Guid id);
    }
}
