using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Operations;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Domains
{
    public class OperationTests
    {
        private static readonly DateTime Now = FixedTimeProvider.DefaultNow.UtcDateTime;

        [Fact]
        public void Add_WithValidData_ShouldSetFields()
        {
            var productionOrderId = Guid.NewGuid();
            var workCenterId = Guid.NewGuid();

            var operation = Operation.Add(productionOrderId, "Montagem", workCenterId, OperationStatus.Pending, Now);

            Assert.NotEqual(Guid.Empty, operation.Id);
            Assert.Equal(productionOrderId, operation.ProductionOrderId);
            Assert.Equal("Montagem", operation.Name);
            Assert.Equal(workCenterId, operation.WorkCenterId);
            Assert.Equal(OperationStatus.Pending, operation.Status);
            Assert.Equal(Now, operation.CreatedAt);
        }

        [Fact]
        public void Add_ShouldTrimOperationName()
        {
            var operation = Operation.Add(Guid.NewGuid(), "  Montagem  ", Guid.NewGuid(), OperationStatus.Pending, Now);

            Assert.Equal("Montagem", operation.Name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(99)]
        public void Add_WithInvalidStatus_ShouldThrowDomainException(int status)
        {
            var exception = Assert.Throws<DomainException>(() => Operation.Add(
                Guid.NewGuid(), "Montagem", Guid.NewGuid(), (OperationStatus)status, Now));

            Assert.Equal(OperationMessages.StatusInvalid, exception.Message);
        }

        [Fact]
        public void Add_WithEmptyRelatedIds_ShouldThrowDomainException()
        {
            Assert.Equal(OperationMessages.ProductionOrderRequired,
                Assert.Throws<DomainException>(() => Operation.Add(Guid.Empty, "Montagem", Guid.NewGuid(), OperationStatus.Pending, Now)).Message);
            Assert.Equal(OperationMessages.WorkCenterRequired,
                Assert.Throws<DomainException>(() => Operation.Add(Guid.NewGuid(), "Montagem", Guid.Empty, OperationStatus.Pending, Now)).Message);
        }

        [Fact]
        public void Update_WithValidData_ShouldKeepIdAndCreatedAt()
        {
            var operation = Operation.Add(Guid.NewGuid(), "Montagem", Guid.NewGuid(), OperationStatus.Pending, Now);
            var id = operation.Id;

            operation.Update(Guid.NewGuid(), "Inspeção", Guid.NewGuid(), OperationStatus.Completed);

            Assert.Equal(id, operation.Id);
            Assert.Equal(Now, operation.CreatedAt);
            Assert.Equal("Inspeção", operation.Name);
            Assert.Equal(OperationStatus.Completed, operation.Status);
        }
    }
}
