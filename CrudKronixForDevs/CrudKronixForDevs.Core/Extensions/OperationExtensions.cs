using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Operations;

namespace CrudKronixForDevs.Core.Extensions
{
    public static class OperationExtensions
    {
        public static OperationResponse ToResponse(this Operation operation) => new(
            operation.Id,
            operation.ProductionOrderId,
            operation.Name,
            operation.WorkCenterId,
            operation.Status,
            operation.CreatedAt);
    }
}
