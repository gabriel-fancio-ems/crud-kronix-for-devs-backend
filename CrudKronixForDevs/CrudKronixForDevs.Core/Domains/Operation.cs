using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Operations;
using System.ComponentModel.DataAnnotations;

namespace CrudKronixForDevs.Core.Domains
{
    public class Operation
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid ProductionOrderId { get; private set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; private set; } = string.Empty;

        public Guid WorkCenterId { get; private set; }

        public OperationStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private Operation() { }

        public static Operation Add(Guid productionOrderId, string name, Guid workCenterId, OperationStatus status, DateTime createdAt)
        {
            var operation = new Operation
            {
                Id = Guid.NewGuid(),
                CreatedAt = createdAt
            };

            operation.DefineData(productionOrderId, name, workCenterId, status);
            return operation;
        }

        public void Update(Guid productionOrderId, string name, Guid workCenterId, OperationStatus status)
        {
            DefineData(productionOrderId, name, workCenterId, status);
        }

        private void DefineData(Guid productionOrderId, string name, Guid workCenterId, OperationStatus status)
        {
            if (productionOrderId == Guid.Empty) throw new DomainException(OperationMessages.ProductionOrderRequired);
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException(OperationMessages.NameRequired);
            if (name.Trim().Length > 150) throw new DomainException(OperationMessages.NameTooLong);
            if (workCenterId == Guid.Empty) throw new DomainException(OperationMessages.WorkCenterRequired);
            if (!Enum.IsDefined(status)) throw new DomainException(OperationMessages.StatusInvalid);

            ProductionOrderId = productionOrderId;
            Name = name.Trim();
            WorkCenterId = workCenterId;
            Status = status;
        }
    }
}
