using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Dtos.Operations;
using CrudKronixForDevs.Core.Resources.Operations;
using CrudKronixForDevs.Tests.Builder;

namespace CrudKronixForDevs.Tests.Unit.Validation
{
    public class OperationRequestValidatorTests
    {
        private readonly OperationRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldReturnNoErrors() =>
            Assert.Empty(_validator.Validate(new OperationRequestBuilder().Build()));

        [Fact]
        public void Validate_EmptyRequest_ShouldReturnAllRequiredFields()
        {
            var errors = _validator.Validate(new OperationRequest());

            Assert.Equal(new[] { "operation", "productionOrderId", "status", "workCenterId" }, errors.Keys.Order());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithoutOperation_ShouldReturnRequiredError(string? value)
        {
            var errors = _validator.Validate(new OperationRequestBuilder().WithOperation(value).Build());

            Assert.Equal(OperationMessages.NameRequired, Assert.Single(errors["operation"]));
        }

        [Fact]
        public void Validate_OperationOver150Characters_ShouldReturnMaximumSizeError()
        {
            var errors = _validator.Validate(new OperationRequestBuilder().WithOperation(new string('A', 151)).Build());

            Assert.Equal(OperationMessages.NameTooLong, Assert.Single(errors["operation"]));
        }

        [Fact]
        public void Validate_WithUnknownStatus_ShouldReturnInvalidError()
        {
            var errors = _validator.Validate(new OperationRequestBuilder().WithStatus((OperationStatus)99).Build());

            Assert.Equal(OperationMessages.StatusInvalid, Assert.Single(errors["status"]));
        }

        [Fact]
        public void Validate_WithEmptyIds_ShouldReturnRequiredErrors()
        {
            var errors = _validator.Validate(new OperationRequestBuilder()
                .WithProductionOrderId(Guid.Empty)
                .WithWorkCenterId(Guid.Empty)
                .Build());

            Assert.Contains("productionOrderId", errors.Keys);
            Assert.Contains("workCenterId", errors.Keys);
        }
    }
}
