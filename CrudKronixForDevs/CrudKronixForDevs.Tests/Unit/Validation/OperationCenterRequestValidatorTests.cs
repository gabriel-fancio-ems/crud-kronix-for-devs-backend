using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Dtos.OperationCenters;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using CrudKronixForDevs.Tests.Builder;

namespace CrudKronixForDevs.Tests.Unit.Validation
{
    public class OperationCenterRequestValidatorTests
    {
        private readonly OperationCenterRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldReturnNoErrors()
        {
            Assert.Empty(_validator.Validate(new OperationCenterRequestBuilder().Build()));
        }

        [Fact]
        public void Validate_EmptyRequest_ShouldReturnAllRequiredFields()
        {
            var errors = _validator.Validate(new OperationCenterRequest());

            Assert.Equal(new[] { "descricao", "status" }.Order(), errors.Keys.Order());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithoutDescription_ShouldReturnRequiredError(string? description)
        {
            var errors = _validator.Validate(new OperationCenterRequestBuilder().WithDescription(description).Build());

            Assert.Equal(OperationCenterMessages.DescriptionRequired, Assert.Single(errors["descricao"]));
        }

        [Fact]
        public void Validate_DescriptionWith150Characters_ShouldBeValid()
        {
            var errors = _validator.Validate(new OperationCenterRequestBuilder().WithDescription(new string('A', 150)).Build());

            Assert.False(errors.ContainsKey("descricao"));
        }

        [Fact]
        public void Validate_DescriptionWith151Characters_ShouldReturnMaximumSizeError()
        {
            var errors = _validator.Validate(new OperationCenterRequestBuilder().WithDescription(new string('A', 151)).Build());

            Assert.Equal(OperationCenterMessages.MaximumSize("descricao", 150), Assert.Single(errors["descricao"]));
        }

        [Fact]
        public void Validate_WithoutStatus_ShouldReturnRequiredError()
        {
            var errors = _validator.Validate(new OperationCenterRequestBuilder().WithStatus(null).Build());

            Assert.Equal(OperationCenterMessages.StatusRequired, Assert.Single(errors["status"]));
        }

        [Fact]
        public void Validate_WithUnknownStatus_ShouldReturnInvalidError()
        {
            var errors = _validator.Validate(new OperationCenterRequestBuilder().WithStatus((OperationCenterStatus)99).Build());

            Assert.Equal(OperationCenterMessages.StatusInvalid, Assert.Single(errors["status"]));
        }
    }
}
