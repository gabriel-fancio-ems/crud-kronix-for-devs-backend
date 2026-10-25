using CrudKronixForDevs.Core.Applications;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Medications;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using CrudKronixForDevs.Core.Services.Medications;
using CrudKronixForDevs.Core.Services.OperationCenters;
using CrudKronixForDevs.Tests.Builder;
using CrudKronixForDevs.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Unit.Applications
{
    /// <summary>
    /// Testes dos casos de uso de medicamentos.
    ///
    /// Aqui usamos o repositório em memória DE VERDADE: ele é rápido, não depende de nada externo
    /// e deixa os testes simples. O relógio é o FixedTimeProvider, para as datas não variarem.
    ///
    /// O xUnit cria uma instância NOVA desta classe para cada teste. Por isso o construtor
    /// funciona como "preparação": cada teste começa com um repositório vazio.
    /// </summary>
    public class MedicationApplicationTests
    {
        private readonly MedicationRepositoryInMemory _repository = new();
        private readonly OperationCenterRepositoryInMemory _operationCenterRepository = new();
        private readonly FixedTimeProvider _clock = new();
        private readonly MedicationApplication _application;
        private readonly OperationCenter _operationCenter;

        public MedicationApplicationTests()
        {
            _application = new MedicationApplication(_repository, _operationCenterRepository, _clock);

            // Todo medicamento precisa de um centro de operação existente.
            // O repositório em memória termina na hora, então não há problema em esperar aqui.
            _operationCenter = OperationCenter.Add("Centro Principal", OperationCenterStatus.Active, _clock.GetUtcNow().UtcDateTime);
            _operationCenterRepository.AddAsync(_operationCenter).GetAwaiter().GetResult();
        }

        // Builder já com a "data de hoje" do relógio fixo e um centro de operação cadastrado.
        private MedicationRequestBuilder NewRequest() => new MedicationRequestBuilder(_clock.Today).WithOperationCenterId(_operationCenter.Id);

        // ---------------------------------------------------------------- CreateAsync

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldSaveAndReturnMedication()
        {
            // Arrange
            var request = NewRequest().WithName("Dipirona").WithPrice(12.90m).Build();

            // Act
            var created = await _application.CreateAsync(request);

            // Assert
            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal("Dipirona", created.Name);
            Assert.Equal(12.90m, created.Price);
            Assert.Equal(_clock.GetUtcNow().UtcDateTime, created.CreatedAt);

            var saved = await _repository.GetByIdAsync(created.Id);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task CreateAsync_ShouldMapQuantityInStockToStockQuantity()
        {
            // No request o campo se chama QuantityInStock; na resposta, StockQuantity.
            var created = await _application.CreateAsync(NewRequest().WithStockQuantity(42).Build());

            Assert.Equal(42, created.StockQuantity);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicatedName_ShouldThrowDuplicateMedicationException()
        {
            await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            var exception = await Assert.ThrowsAsync<DuplicateMedicationException>(
                () => _application.CreateAsync(NewRequest().WithName("Dipirona").Build()));

            Assert.Equal(MedicationMessages.DuplicateMedication("Dipirona"), exception.Message);
        }

        [Theory]
        [InlineData("DIPIRONA")]     // maiúsculas
        [InlineData("  dipirona  ")] // espaços
        [InlineData("Dípirona")]     // acento
        public async Task CreateAsync_DuplicatedNameIgnoringCaseAccentsAndSpaces_ShouldThrow(string duplicatedName)
        {
            await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            await Assert.ThrowsAsync<DuplicateMedicationException>(
                () => _application.CreateAsync(NewRequest().WithName(duplicatedName).Build()));
        }

        [Fact]
        public async Task CreateAsync_WithInvalidData_ShouldThrowDomainExceptionAndNotSave()
        {
            await Assert.ThrowsAsync<DomainException>(
                () => _application.CreateAsync(NewRequest().WithPrice(0).Build()));

            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task CreateAsync_WithoutExpirationDate_ShouldThrowDomainException()
        {
            // Sem data, a aplicação usa default (01/01/0001), que está no passado.
            await Assert.ThrowsAsync<DomainException>(
                () => _application.CreateAsync(NewRequest().WithExpirationDate(null).Build()));
        }

        [Fact]
        public async Task CreateAsync_ShouldLinkMedicationToOperationCenter()
        {
            var created = await _application.CreateAsync(NewRequest().Build());

            Assert.Equal(_operationCenter.Id, created.OperationCenterId);
        }

        [Fact]
        public async Task CreateAsync_WithUnknownOperationCenter_ShouldThrowDomainExceptionAndNotSave()
        {
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _application.CreateAsync(NewRequest().WithOperationCenterId(Guid.NewGuid()).Build()));

            Assert.Equal(OperationCenterMessages.OperationCenterNotFound, exception.Message);
            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task CreateAsync_WithoutOperationCenter_ShouldThrowDomainException()
        {
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _application.CreateAsync(NewRequest().WithOperationCenterId(null).Build()));

            Assert.Equal(OperationCenterMessages.OperationCenterRequired, exception.Message);
        }

        // ---------------------------------------------------------------- GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnMedication()
        {
            var created = await _application.CreateAsync(NewRequest().Build());

            var found = await _application.GetByIdAsync(created.Id);

            Assert.NotNull(found);
            Assert.Equal(created, found); // records comparam pelo conteúdo
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ShouldReturnNull()
        {
            var found = await _application.GetByIdAsync(Guid.NewGuid());

            Assert.Null(found);
        }

        // ---------------------------------------------------------------- ListAsync

        [Fact]
        public async Task ListAsync_WithoutFilter_ShouldReturnAllOrderedByName()
        {
            await _application.CreateAsync(NewRequest().WithName("Omeprazol").Build());
            await _application.CreateAsync(NewRequest().WithName("Amoxicilina").Build());
            await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            var result = await _application.ListAsync(null);

            Assert.Equal(new[] { "Amoxicilina", "Dipirona", "Omeprazol" }, result.Select(m => m.Name));
        }

        [Fact]
        public async Task ListAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            var result = await _application.ListAsync(null);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("acido")]     // sem acento
        [InlineData("ÁCIDO")]     // maiúsculas com acento
        [InlineData("acetilsal")] // trecho do meio
        public async Task ListAsync_FilterByName_ShouldIgnoreCaseAndAccents(string filter)
        {
            await _application.CreateAsync(NewRequest().WithName("Ácido Acetilsalicílico").WithActiveIngredient("AAS").Build());
            await _application.CreateAsync(NewRequest().WithName("Omeprazol").WithActiveIngredient("Omeprazol").Build());

            var result = await _application.ListAsync(filter);

            var single = Assert.Single(result);
            Assert.Equal("Ácido Acetilsalicílico", single.Name);
        }

        [Fact]
        public async Task ListAsync_FilterByActiveIngredient_ShouldFindMedication()
        {
            await _application.CreateAsync(NewRequest().WithName("Novalgina").WithActiveIngredient("Dipirona").Build());
            await _application.CreateAsync(NewRequest().WithName("Tylenol").WithActiveIngredient("Paracetamol").Build());

            var result = await _application.ListAsync("dipirona");

            Assert.Equal("Novalgina", Assert.Single(result).Name);
        }

        [Fact]
        public async Task ListAsync_FilterWithoutMatches_ShouldReturnEmptyList()
        {
            await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            var result = await _application.ListAsync("inexistente");

            Assert.Empty(result);
        }

        // ---------------------------------------------------------------- UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldChangeMedication()
        {
            var created = await _application.CreateAsync(NewRequest().WithName("Dipirona").WithPrice(10m).Build());

            var updated = await _application.UpdateAsync(created.Id,
                NewRequest().WithName("Dipirona").WithPrice(25m).Build());

            Assert.NotNull(updated);
            Assert.Equal(25m, updated.Price);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(25m, (await _repository.GetByIdAsync(created.Id))!.Price);
        }

        [Fact]
        public async Task UpdateAsync_KeepingTheSameName_ShouldNotBeConsideredDuplicate()
        {
            // A checagem de duplicidade ignora o próprio medicamento que está sendo atualizado.
            var created = await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            var updated = await _application.UpdateAsync(created.Id, NewRequest().WithName("Dipirona").Build());

            Assert.NotNull(updated);
        }

        [Fact]
        public async Task UpdateAsync_UsingNameOfAnotherMedication_ShouldThrowDuplicateMedicationException()
        {
            await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());
            var omeprazol = await _application.CreateAsync(NewRequest().WithName("Omeprazol").Build());

            await Assert.ThrowsAsync<DuplicateMedicationException>(
                () => _application.UpdateAsync(omeprazol.Id, NewRequest().WithName("Dipirona").Build()));
        }

        [Fact]
        public async Task UpdateAsync_ChangingToAnotherOperationCenter_ShouldUpdateTheLink()
        {
            var other = OperationCenter.Add("Centro Secundário", OperationCenterStatus.Active, _clock.GetUtcNow().UtcDateTime);
            await _operationCenterRepository.AddAsync(other);
            var created = await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            var updated = await _application.UpdateAsync(created.Id,
                NewRequest().WithName("Dipirona").WithOperationCenterId(other.Id).Build());

            Assert.Equal(other.Id, updated!.OperationCenterId);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownOperationCenter_ShouldThrowAndKeepOriginalLink()
        {
            var created = await _application.CreateAsync(NewRequest().WithName("Dipirona").Build());

            await Assert.ThrowsAsync<DomainException>(
                () => _application.UpdateAsync(created.Id,
                    NewRequest().WithName("Dipirona").WithOperationCenterId(Guid.NewGuid()).Build()));

            Assert.Equal(_operationCenter.Id, (await _repository.GetByIdAsync(created.Id))!.OperationCenterId);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ShouldReturnNull()
        {
            var updated = await _application.UpdateAsync(Guid.NewGuid(), NewRequest().Build());

            Assert.Null(updated);
        }

        // ---------------------------------------------------------------- RemoveAsync

        [Fact]
        public async Task RemoveAsync_WithExistingId_ShouldRemoveAndReturnTrue()
        {
            var created = await _application.CreateAsync(NewRequest().Build());

            var removed = await _application.RemoveAsync(created.Id);

            Assert.True(removed);
            Assert.Null(await _application.GetByIdAsync(created.Id));
        }

        [Fact]
        public async Task RemoveAsync_WithUnknownId_ShouldReturnFalse()
        {
            Assert.False(await _application.RemoveAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task RemoveAsync_SameIdTwice_SecondCallShouldReturnFalse()
        {
            var created = await _application.CreateAsync(NewRequest().Build());

            Assert.True(await _application.RemoveAsync(created.Id));
            Assert.False(await _application.RemoveAsync(created.Id));
        }

        // ---------------------------------------------------------------- ListItemsNearExpiration

        [Fact]
        public async Task ListItemsNearExpiration_ShouldReturnOnlyItemsInsideTheWindow()
        {
            // Arrange: um vence em 5 dias, outro em 200 dias.
            await _application.CreateAsync(NewRequest().WithName("Vence logo").WithExpirationDate(_clock.Today.AddDays(5)).Build());
            await _application.CreateAsync(NewRequest().WithName("Vence longe").WithExpirationDate(_clock.Today.AddDays(200)).Build());

            // Act: pede só os que vencem nos próximos 30 dias.
            var result = await _application.ListItemsNearExpiration(30);

            // Assert
            Assert.Equal("Vence logo", Assert.Single(result).Name);
        }

        [Fact]
        public async Task ListItemsNearExpiration_ShouldOrderByExpirationDate()
        {
            await _application.CreateAsync(NewRequest().WithName("Em 20 dias").WithExpirationDate(_clock.Today.AddDays(20)).Build());
            await _application.CreateAsync(NewRequest().WithName("Em 3 dias").WithExpirationDate(_clock.Today.AddDays(3)).Build());

            var result = await _application.ListItemsNearExpiration(30);

            Assert.Equal(new[] { "Em 3 dias", "Em 20 dias" }, result.Select(m => m.Name));
        }
    }
}
