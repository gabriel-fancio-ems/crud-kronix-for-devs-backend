using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Medications;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using CrudKronixForDevs.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Domains
{
    /// <summary>
    /// Testes da entidade Medication: as regras de negócio que valem SEMPRE.
    ///
    /// Todos seguem o padrão AAA:
    ///   Arrange → preparar os dados
    ///   Act     → executar o que está sendo testado
    ///   Assert  → conferir o resultado
    ///
    /// Nome dos testes: Metodo_Cenario_ResultadoEsperado.
    /// </summary>
    public class MedicationTests
    {
        private static readonly DateTime Now = FixedTimeProvider.DefaultNow.UtcDateTime;
        private static readonly DateOnly Today = DateOnly.FromDateTime(Now);
        private static readonly Guid OperationCenterId = Guid.NewGuid();

        // Cria um medicamento válido; cada teste muda só o parâmetro que interessa.
        private static Medication CreateValid(
            string name = "Dipirona",
            string activeIngredient = "Dipirona",
            decimal price = 10m,
            int stockQuantity = 5,
            DateOnly? expirationDate = null,
            Guid? operationCenterId = null) =>
            Medication.Add(name, activeIngredient, price, stockQuantity, expirationDate ?? Today.AddYears(1),
                operationCenterId ?? OperationCenterId, Now);

        // ---------------------------------------------------------------- Add (criação)

        [Fact]
        public void Add_WithValidData_ShouldFillAllProperties()
        {
            // Arrange
            var expiration = Today.AddMonths(6);

            // Act
            var medication = Medication.Add("Dipirona", "Dipirona Monoidratada", 12.90m, 100, expiration, OperationCenterId, Now);

            // Assert
            Assert.NotEqual(Guid.Empty, medication.Id);
            Assert.Equal("Dipirona", medication.Name);
            Assert.Equal("Dipirona Monoidratada", medication.ActiveIngredient);
            Assert.Equal(12.90m, medication.Price);
            Assert.Equal(100, medication.StockQuantity);
            Assert.Equal(expiration, medication.ExpirationDate);
            Assert.Equal(OperationCenterId, medication.OperationCenterId);
            Assert.Equal(Now, medication.CreatedAt);
        }

        [Fact]
        public void Add_WithoutOperationCenter_ShouldThrowDomainException()
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(operationCenterId: Guid.Empty));

            Assert.Equal(OperationCenterMessages.OperationCenterRequired, exception.Message);
        }

        [Fact]
        public void Add_ShouldTrimSpacesFromTexts()
        {
            var medication = CreateValid(name: "  Dipirona  ", activeIngredient: "  Dipirona  ");

            Assert.Equal("Dipirona", medication.Name);
            Assert.Equal("Dipirona", medication.ActiveIngredient);
        }

        [Fact]
        public void Add_TwoMedications_ShouldGenerateDifferentIds()
        {
            var first = CreateValid();
            var second = CreateValid();

            Assert.NotEqual(first.Id, second.Id);
        }

        // [Theory] roda o mesmo teste várias vezes, uma para cada [InlineData].
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Add_WithoutName_ShouldThrowDomainException(string? name)
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(name: name!));

            Assert.Equal(MedicationMessages.NameRequired, exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Add_WithoutActiveIngredient_ShouldThrowDomainException(string? activeIngredient)
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(activeIngredient: activeIngredient!));

            Assert.Equal(MedicationMessages.ActiveIngredientRequired, exception.Message);
        }

        // decimal não pode ir em [InlineData], então usamos TheoryData + [MemberData].
        public static TheoryData<decimal> InvalidPrices => new() { 0m, -0.01m, -100m };

        [Theory]
        [MemberData(nameof(InvalidPrices))]
        public void Add_WithPriceZeroOrNegative_ShouldThrowDomainException(decimal price)
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(price: price));

            Assert.Equal(MedicationMessages.PriceInvalid, exception.Message);
        }

        [Fact]
        public void Add_WithMinimumPositivePrice_ShouldSucceed()
        {
            var medication = CreateValid(price: 0.01m);

            Assert.Equal(0.01m, medication.Price);
        }

        [Fact]
        public void Add_WithNegativeStock_ShouldThrowDomainException()
        {
            var exception = Assert.Throws<DomainException>(() => CreateValid(stockQuantity: -1));

            Assert.Equal(MedicationMessages.StockInvalid, exception.Message);
        }

        [Fact]
        public void Add_WithZeroStock_ShouldSucceed()
        {
            // Estoque zerado é permitido: o medicamento existe, só está em falta.
            var medication = CreateValid(stockQuantity: 0);

            Assert.Equal(0, medication.StockQuantity);
        }

        [Theory]
        [InlineData(0)]    // vence hoje
        [InlineData(-1)]   // venceu ontem
        [InlineData(-365)] // venceu há um ano
        public void Add_WithExpirationTodayOrInThePast_ShouldThrowDomainException(int daysFromToday)
        {
            var exception = Assert.Throws<DomainException>(
                () => CreateValid(expirationDate: Today.AddDays(daysFromToday)));

            Assert.Equal(MedicationMessages.ExpirationDateMustBeFuture, exception.Message);
        }

        [Fact]
        public void Add_WithExpirationTomorrow_ShouldSucceed()
        {
            var medication = CreateValid(expirationDate: Today.AddDays(1));

            Assert.Equal(Today.AddDays(1), medication.ExpirationDate);
        }

        // ---------------------------------------------------------------- Update

        [Fact]
        public void Update_WithValidData_ShouldChangeDataAndKeepIdAndCreatedAt()
        {
            // Arrange
            var medication = CreateValid();
            var originalId = medication.Id;
            var later = Now.AddDays(10);

            // Act
            var newOperationCenterId = Guid.NewGuid();
            medication.Update("Dipirona Sódica", "Dipirona", 15m, 20, Today.AddYears(2), newOperationCenterId, later);

            // Assert
            Assert.Equal("Dipirona Sódica", medication.Name);
            Assert.Equal(15m, medication.Price);
            Assert.Equal(20, medication.StockQuantity);
            Assert.Equal(Today.AddYears(2), medication.ExpirationDate);
            Assert.Equal(newOperationCenterId, medication.OperationCenterId);
            Assert.Equal(originalId, medication.Id);   // o id não muda
            Assert.Equal(Now, medication.CreatedAt);   // a data de criação não muda
        }

        [Fact]
        public void Update_WithInvalidData_ShouldThrowAndKeepOriginalData()
        {
            var medication = CreateValid(price: 10m);

            Assert.Throws<DomainException>(
                () => medication.Update("Dipirona", "Dipirona", -5m, 10, Today.AddYears(1), OperationCenterId, Now));

            // As validações acontecem ANTES de qualquer alteração, então nada mudou.
            Assert.Equal(10m, medication.Price);
        }

        // ---------------------------------------------------------------- IsExpired / ExpiresSoon

        [Fact]
        public void IsExpired_BeforeExpirationDate_ShouldBeFalse()
        {
            var medication = CreateValid(expirationDate: Today.AddDays(10));

            Assert.False(medication.IsExpired(Today));
        }

        [Fact]
        public void IsExpired_AfterExpirationDate_ShouldBeTrue()
        {
            var medication = CreateValid(expirationDate: Today.AddDays(10));

            Assert.True(medication.IsExpired(Today.AddDays(11)));
        }

        [Fact]
        public void IsExpired_OnExpirationDate_ShouldBeFalse()
        {
            // No próprio dia da validade o medicamento ainda pode ser usado.
            var medication = CreateValid(expirationDate: Today.AddDays(10));

            Assert.False(medication.IsExpired(Today.AddDays(10)));
        }

        [Theory]
        [InlineData(5, true)]    // vence em 5 dias  → dentro da janela de 30
        [InlineData(30, true)]   // vence no último dia da janela
        [InlineData(31, false)]  // fora da janela
        [InlineData(200, false)] // bem longe
        public void ExpiresSoon_ShouldConsiderOnlyTheWindow(int daysUntilExpiration, bool expected)
        {
            var medication = CreateValid(expirationDate: Today.AddDays(daysUntilExpiration));

            Assert.Equal(expected, medication.ExpiresSoon(30, Today));
        }

        [Fact]
        public void ExpiresSoon_WhenAlreadyExpired_ShouldBeFalse()
        {
            // Vencido não é "vence em breve": é outro caso (IsExpired).
            var medication = CreateValid(expirationDate: Today.AddDays(5));

            Assert.False(medication.ExpiresSoon(30, Today.AddDays(6)));
        }
    }
}
