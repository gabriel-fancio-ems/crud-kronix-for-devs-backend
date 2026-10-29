using CrudKronixForDevs.Core.Dtos.Equipments;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    /// <summary>
    /// Casos de uso de equipamentos.
    /// O Controller conversa apenas com esta interface.
    /// </summary>
    public interface IEquipmentApplication
    {
        Task<IReadOnlyList<EquipmentResponse>> ListAsync();
        Task<EquipmentResponse> CreateAsync(EquipmentRequest request);
    }
}
