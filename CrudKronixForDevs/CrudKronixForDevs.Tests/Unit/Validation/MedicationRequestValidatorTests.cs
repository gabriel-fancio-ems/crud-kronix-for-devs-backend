using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Dtos.Medications;
using CrudKronixForDevs.Core.Resources.Medications;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using CrudKronixForDevs.Tests.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Validation
{
    /// <summary>
    /// O validador confere o FORMATO do JSON e devolve todos os erros de uma vez.
    /// As chaves do dicionário são os nomes de campo que o cliente da API recebe no 400.
    /// </summary>
    public class MedicationRequestValidatorTests
    {
        private readonly MedicationRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldReturnNoErrors()
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().Build());

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_EmptyRequest_ShouldReturnAllRequiredFields()
        {
            var errors = _validator.Validate(new MedicationRequest());

            Assert.Equal(
                new[] { "nome", "principioAtivo", "preco", "dataValidade", "centroOperacao" }.Order(),
                errors.Keys.Order());
        }

        public static TheoryData<Guid?> MissingOperationCenterIds => new() { null, Guid.Empty };

        [Theory]
        [MemberData(nameof(MissingOperationCenterIds))]
        public void Validate_WithoutOperationCenter_ShouldReturnRequiredError(Guid? operationCenterId)
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithOperationCenterId(operationCenterId).Build());

            Assert.Equal(OperationCenterMessages.OperationCenterRequired, Assert.Single(errors["centroOperacao"]));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithoutName_ShouldReturnRequiredError(string? name)
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithName(name).Build());

            Assert.Equal(MedicationMessages.NameRequired, Assert.Single(errors["nome"]));
        }

        [Fact]
        public void Validate_NameWith150Characters_ShouldBeValid()
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithName(new string('A', 150)).Build());

            Assert.False(errors.ContainsKey("nome"));
        }

        [Fact]
        public void Validate_NameWith151Characters_ShouldReturnMaximumSizeError()
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithName(new string('A', 151)).Build());

            Assert.Equal(MedicationMessages.MaximumSize("nome", 150), Assert.Single(errors["nome"]));
        }

        [Fact]
        public void Validate_ActiveIngredientTooLong_ShouldReturnError()
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithActiveIngredient(new string('A', 151)).Build());

            Assert.True(errors.ContainsKey("principioAtivo"));
        }

        public static TheoryData<decimal> InvalidPrices => new() { 0m, -1m, 100_000.01m };

        [Theory]
        [MemberData(nameof(InvalidPrices))]
        public void Validate_InvalidPrice_ShouldReturnPriceError(decimal price)
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithPrice(price).Build());

            Assert.True(errors.ContainsKey("preco"));
        }

        public static TheoryData<decimal> ValidPrices => new() { 0.01m, 12.90m, 100_000m };

        [Theory]
        [MemberData(nameof(ValidPrices))]
        public void Validate_ValidPrice_ShouldNotReturnPriceError(decimal price)
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithPrice(price).Build());

            Assert.False(errors.ContainsKey("preco"));
        }

        [Fact]
        public void Validate_NegativeStock_ShouldReturnError()
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithStockQuantity(-1).Build());

            Assert.Equal(MedicationMessages.StockInvalid, Assert.Single(errors["quantidadeEstoque"]));
        }

        [Fact]
        public void Validate_WithoutExpirationDate_ShouldReturnError()
        {
            var errors = _validator.Validate(new MedicationRequestBuilder().WithExpirationDate(null).Build());

            Assert.Equal(MedicationMessages.ExpirationDateRequired, Assert.Single(errors["dataValidade"]));
        }

        [Fact]
        public void Validate_PastExpirationDate_ShouldNotBeRejectedHere()
        {
            // Data no passado é REGRA DE NEGÓCIO: quem barra é a entidade Medication (vira 400 pelo middleware).
            // O validador só confere se a data foi enviada.
            var errors = _validator.Validate(new MedicationRequestBuilder()
                .WithExpirationDate(DateOnly.FromDateTime(DateTime.Today).AddDays(-1))
                .Build());

            Assert.Empty(errors);
        }
    }
}
