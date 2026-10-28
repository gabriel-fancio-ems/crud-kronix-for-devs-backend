using CrudKronixForDevs.Core.Domains.Enums;

namespace CrudKronixForDevs.Core.Dtos.Operations
{
    public record OperationResponse(
        Guid Id,
        Guid ProductionOrderId,
        string Operation,
        Guid WorkCenterId,
        OperationStatus Status,
        DateTime CreatedAt);
}
