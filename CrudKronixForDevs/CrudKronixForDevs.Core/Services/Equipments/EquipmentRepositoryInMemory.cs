using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Interfaces;
using System.Collections.Concurrent;

namespace CrudKronixForDevs.Core.Services.Equipments
{
    /// <summary>
    /// "Banco de dados" em memória de equipamentos.
    /// Registrado como Singleton, pelo mesmo motivo do MedicationRepositoryInMemory.
    /// </summary>
    public class EquipmentRepositoryInMemory : IEquipmentRepository
    {
        private readonly ConcurrentDictionary<Guid, Equipment> _equipments = new();

        public Task<IReadOnlyList<Equipment>> GetAllAsync()
        {
            IReadOnlyList<Equipment> list = _equipments.Values.ToList();
            return Task.FromResult(list);
        }

        public Task AddAsync(Equipment equipment)
        {
            _equipments[equipment.Id] = equipment;
            return Task.CompletedTask;
        }
    }
}
