using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.OperationCenters;

namespace CrudKronixForDevs.Core.Extensions
{
    /// <summary>
    /// Converte a entidade OperationCenter em DTO de saída.
    /// </summary>
    public static class OperationCenterExtensions
    {
        public static OperationCenterResponse ToResponse(this OperationCenter operationCenter) => new(
            operationCenter.Id,
            operationCenter.Description,
            operationCenter.Status,
            operationCenter.CreatedAt
        );
    }
}
