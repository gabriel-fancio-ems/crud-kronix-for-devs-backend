using CrudKronixForDevs.Core.Domains.Enums;

namespace CrudKronixForDevs.Core.Dtos.Equipments
{
    /// <summary>
    /// DTO de ENTRADA: o JSON que o cliente envia no POST.
    /// Os campos são anuláveis para que o validador devolva mensagens claras quando faltarem.
    /// </summary>
    public class EquipmentRequest
    {
        /// <example>EQ-001</example>
        public string? Code { get; init; }

        /// <example>Desfibrilador DEA-200</example>
        public string? Description { get; init; }

        /// <example>Active</example>
        public EquipmentStatus? Status { get; init; }
    }
}
