using CrudKronixForDevs.Core.Applications;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.OperationCenters;
using CrudKronixForDevs.Core.Services.Medications;
using CrudKronixForDevs.Core.Services.OperationCenters;
using CrudKronixForDevs.Tests.Builder;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Applications
{
    /// <summary>
    /// Testes dos casos de uso de centros de operação, com os repositórios em memória de verdade.
    /// </summary>
    public class OperationCenterApplicationTests
    {
        private readonly OperationCenterRepositoryInMemory _repository = new();
        private readonly MedicationRepositoryInMemory _medicationRepository = new();
        private readonly FixedTimeProvider _clock = new();
        private readonly OperationCenterApplication _application;

        public OperationCenterApplicationTests()
        {
            _application = new OperationCenterApplication(_repository, _medicationRepository, _clock);
        }

        private static OperationCenterRequestBuilder NewRequest() => new();

        // ---------------------------------------------------------------- CreateAsync

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldSaveAndReturnOperationCenter()
        {
            var created = await _application.CreateAsync(
                NewRequest().WithDescription("Centro Principal").WithStatus(OperationCenterStatus.Active).Build());

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal("Centro Principal", created.Description);
            Assert.Equal(OperationCenterStatus.Active, created.Status);
            Assert.Equal(_clock.GetUtcNow().UtcDateTime, created.CreatedAt);
            Assert.NotNull(await _repository.GetByIdAsync(created.Id));
        }

        [Theory]
        [InlineData("Centro Principal")]
        [InlineData("CENTRO PRINCIPAL")]
        [InlineData("  centro principal  ")]
        public async Task CreateAsync_WithDuplicatedDescription_ShouldThrowDuplicateOperationCenterException(string duplicated)
        {
            await _application.CreateAsync(NewRequest().WithDescription("Centro Principal").Build());

            await Assert.ThrowsAsync<DuplicateOperationCenterException>(
                () => _application.CreateAsync(NewRequest().WithDescription(duplicated).Build()));
        }

        [Fact]
        public async Task CreateAsync_WithoutStatus_ShouldThrowDomainExceptionAndNotSave()
        {
            await Assert.ThrowsAsync<DomainException>(
                () => _application.CreateAsync(NewRequest().WithStatus(null).Build()));

            Assert.Empty(await _repository.GetAllAsync());
        }

        // ---------------------------------------------------------------- GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnOperationCenter()
        {
            var created = await _application.CreateAsync(NewRequest().Build());

            Assert.Equal(created, await _application.GetByIdAsync(created.Id));
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ShouldReturnNull()
        {
            Assert.Null(await _application.GetByIdAsync(Guid.NewGuid()));
        }

        // ---------------------------------------------------------------- ListAsync

        [Fact]
        public async Task ListAsync_WithoutFilter_ShouldReturnAllOrderedByDescription()
        {
            await _application.CreateAsync(NewRequest().WithDescription("Recife").Build());
            await _application.CreateAsync(NewRequest().WithDescription("Belém").Build());
            await _application.CreateAsync(NewRequest().WithDescription("Curitiba").Build());

            var result = await _application.ListAsync(null);

            Assert.Equal(new[] { "Belém", "Curitiba", "Recife" }, result.Select(o => o.Description));
        }

        [Fact]
        public async Task ListAsync_FilterByDescription_ShouldIgnoreCaseAndAccents()
        {
            await _application.CreateAsync(NewRequest().WithDescription("Centro São Paulo").Build());
            await _application.CreateAsync(NewRequest().WithDescription("Centro Recife").Build());

            var result = await _application.ListAsync("SAO PAULO");

            Assert.Equal("Centro São Paulo", Assert.Single(result).Description);
        }

        // ---------------------------------------------------------------- UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldChangeOperationCenter()
        {
            var created = await _application.CreateAsync(NewRequest().WithDescription("Centro Principal").Build());

            var updated = await _application.UpdateAsync(created.Id,
                NewRequest().WithDescription("Centro Principal").WithStatus(OperationCenterStatus.Inactive).Build());

            Assert.NotNull(updated);
            Assert.Equal(OperationCenterStatus.Inactive, updated.Status);
            Assert.Equal(created.CreatedAt, updated.CreatedAt);
        }

        [Fact]
        public async Task UpdateAsync_UsingDescriptionOfAnotherOperationCenter_ShouldThrow()
        {
            await _application.CreateAsync(NewRequest().WithDescription("Centro A").Build());
            var second = await _application.CreateAsync(NewRequest().WithDescription("Centro B").Build());

            await Assert.ThrowsAsync<DuplicateOperationCenterException>(
                () => _application.UpdateAsync(second.Id, NewRequest().WithDescription("Centro A").Build()));
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ShouldReturnNull()
        {
            Assert.Null(await _application.UpdateAsync(Guid.NewGuid(), NewRequest().Build()));
        }

        // ---------------------------------------------------------------- RemoveAsync

        [Fact]
        public async Task RemoveAsync_WithExistingId_ShouldRemoveAndReturnTrue()
        {
            var created = await _application.CreateAsync(NewRequest().Build());

            Assert.True(await _application.RemoveAsync(created.Id));
            Assert.Null(await _application.GetByIdAsync(created.Id));
        }

        [Fact]
        public async Task RemoveAsync_WithUnknownId_ShouldReturnFalse()
        {
            Assert.False(await _application.RemoveAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task RemoveAsync_WithLinkedMedications_ShouldThrowAndKeepOperationCenter()
        {
            var created = await _application.CreateAsync(NewRequest().WithDescription("Centro Principal").Build());
            var now = _clock.GetUtcNow().UtcDateTime;
            await _medicationRepository.AddAsync(
                Medication.Add("Dipirona", "Dipirona", 10m, 1, _clock.Today.AddYears(1), created.Id, now));

            var exception = await Assert.ThrowsAsync<OperationCenterInUseException>(
                () => _application.RemoveAsync(created.Id));

            Assert.Equal(OperationCenterMessages.OperationCenterInUse("Centro Principal"), exception.Message);
            Assert.NotNull(await _repository.GetByIdAsync(created.Id));
        }
    }
}
