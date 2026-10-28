using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Dtos.Operations;

namespace CrudKronixForDevs.Tests.Builder
{
    public class OperationRequestBuilder
    {
        private Guid? _productionOrderId = Guid.NewGuid();
        private string? _operation = "Montagem";
        private Guid? _workCenterId = Guid.NewGuid();
        private OperationStatus? _status = OperationStatus.Pending;

        public OperationRequestBuilder WithProductionOrderId(Guid? id) { _productionOrderId = id; return this; }
        public OperationRequestBuilder WithOperation(string? operation) { _operation = operation; return this; }
        public OperationRequestBuilder WithWorkCenterId(Guid? id) { _workCenterId = id; return this; }
        public OperationRequestBuilder WithStatus(OperationStatus? status) { _status = status; return this; }
        public OperationRequestBuilder WithUniqueOperation() => WithOperation($"Operação {Guid.NewGuid():N}");

        public OperationRequest Build() => new()
        {
            ProductionOrderId = _productionOrderId,
            Operation = _operation,
            WorkCenterId = _workCenterId,
            Status = _status
        };
    }
}
