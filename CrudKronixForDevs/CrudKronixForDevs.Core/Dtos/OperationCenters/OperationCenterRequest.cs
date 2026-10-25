using CrudKronixForDevs.Core.Domains.Enums;

namespace CrudKronixForDevs.Core.Dtos.OperationCenters
{
    /// <summary>
    /// DTO de ENTRADA: o JSON que o cliente envia no POST e no PUT.
    /// Os campos são anuláveis para que o validador devolva mensagens claras quando faltarem.
    /// </summary>
    public class OperationCenterRequest
    {
        /// <example>Centro de Distribuição São Paulo</example>
        public string? Description { get; init; }

        /// <example>Active</example>
        public OperationCenterStatus? Status { get; init; }
    }
}
