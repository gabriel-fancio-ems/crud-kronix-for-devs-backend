using CrudKronixForDevs.Core.Domains.Enums;

namespace CrudKronixForDevs.Core.Dtos.Equipments
{
    /// <summary>
    /// DTO de SAÍDA: o JSON que a API devolve para um equipamento.
    /// </summary>
    public record EquipmentResponse(
        Guid Id,
        string Code,
        string Description,
        EquipmentStatus Status,
        DateTime CreatedAt);
}
