using CrudKronixForDevs.Core.Domains.Enums;

namespace CrudKronixForDevs.Core.Dtos.OperationCenters
{
    /// <summary>
    /// DTO de SAÍDA: o JSON que a API devolve para um centro de operação.
    /// </summary>
    public record OperationCenterResponse(
        Guid Id,
        string Description,
        OperationCenterStatus Status,
        DateTime CreatedAt);
}
