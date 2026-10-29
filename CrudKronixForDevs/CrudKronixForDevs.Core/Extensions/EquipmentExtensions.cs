using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Equipments;

namespace CrudKronixForDevs.Core.Extensions
{
    /// <summary>
    /// Converte a entidade Equipment em DTO de saída.
    /// </summary>
    public static class EquipmentExtensions
    {
        public static EquipmentResponse ToResponse(this Equipment equipment) => new(
            equipment.Id,
            equipment.Code,
            equipment.Description,
            equipment.Status,
            equipment.CreatedAt
        );
    }
}
