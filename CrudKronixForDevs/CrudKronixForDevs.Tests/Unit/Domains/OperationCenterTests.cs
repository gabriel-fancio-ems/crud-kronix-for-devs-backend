using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Domains
{
    /// <summary>
    /// Testes da entidade OperationCenter: as regras de negócio que valem SEMPRE.
    /// </summary>
    public class OperationCenterTests
    {
        private static readonly DateTime Now = FixedTimeProvider.DefaultNow.UtcDateTime;

        private static OperationCenter CreateValid(
            string description = "Centro Principal",
            OperationCenterStatus status = OperationCenterStatus.Active) =>
            OperationCenter.Add(description, status, Now);

        // ---------------------------------------------------------------- Add (criação)

        [Fact]
        public void Add_WithValidData_ShouldFillAllProperties()
        {
            var operationCenter = OperationCenter.Add("Centro Principal", OperationCenterStatus.Inactive, Now);

            Assert.NotEqual(Guid.Empty, operationCenter.Id);
            Assert.Equal("Centro Principal", operationCenter.Description);
            Assert.Equal(OperationCenterStatus.Inactive, operationCenter.Status);
            Assert.Equal(Now, operationCenter.CreatedAt);
        }

        [Fact]
        public void Add_ShouldTrimSpacesFromDescription()
        {
            var operationCenter = CreateValid(description: "  Centro Principal  ");

            Assert.Equal("Centro Principal", operationCenter.Description);
        }

        [Fact]
        public void Add_TwoOperationCenters_ShouldGenerateDifferentIds()
        {
            Assert.NotEqual(CreateValid().Id, CreateValid().Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Add_WithoutDescription_ShouldThrowDomainException(string? description)
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(description: description!));

            Assert.Equal(OperationCenterMessages.DescriptionRequired, exception.Message);
        }

        [Theory]
        [InlineData(0)]   // default do enum: o que chega quando o status não é enviado
        [InlineData(99)]  // valor inexistente
        public void Add_WithInvalidStatus_ShouldThrowDomainException(int status)
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(status: (OperationCenterStatus)status));

            Assert.Equal(OperationCenterMessages.StatusInvalid, exception.Message);
        }

        // ---------------------------------------------------------------- Update

        [Fact]
        public void Update_WithValidData_ShouldChangeDataAndKeepIdAndCreatedAt()
        {
            var operationCenter = CreateValid();
            var originalId = operationCenter.Id;

            operationCenter.Update("Centro Novo", OperationCenterStatus.Inactive);

            Assert.Equal("Centro Novo", operationCenter.Description);
            Assert.Equal(OperationCenterStatus.Inactive, operationCenter.Status);
            Assert.Equal(originalId, operationCenter.Id);
            Assert.Equal(Now, operationCenter.CreatedAt);
        }

        [Fact]
        public void Update_WithInvalidData_ShouldThrowAndKeepOriginalData()
        {
            var operationCenter = CreateValid(description: "Centro Principal");

            Assert.Throws<DomainException>(() => operationCenter.Update("", OperationCenterStatus.Active));

            Assert.Equal("Centro Principal", operationCenter.Description);
        }

        // ---------------------------------------------------------------- IsActive

        [Theory]
        [InlineData(OperationCenterStatus.Active, true)]
        [InlineData(OperationCenterStatus.Inactive, false)]
        public void IsActive_ShouldReflectStatus(OperationCenterStatus status, bool expected)
        {
            Assert.Equal(expected, CreateValid(status: status).IsActive);
        }
    }
}
