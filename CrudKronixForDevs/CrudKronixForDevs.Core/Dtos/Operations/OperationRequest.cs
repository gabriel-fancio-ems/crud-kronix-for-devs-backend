using CrudKronixForDevs.Core.Domains.Enums;

namespace CrudKronixForDevs.Core.Dtos.Operations
{
    public class OperationRequest
    {
        public Guid? ProductionOrderId { get; init; }
        public string? Operation { get; init; }
        public Guid? WorkCenterId { get; init; }
        public OperationStatus? Status { get; init; }
    }
}
