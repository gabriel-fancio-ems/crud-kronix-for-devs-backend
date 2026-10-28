using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Services.Operations;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Services
{
    public class OperationRepositoryInMemoryTests
    {
        private readonly OperationRepositoryInMemory _repository = new();

        private static Operation NewOperation() => Operation.Add(
            Guid.NewGuid(), "Montagem", Guid.NewGuid(), OperationStatus.Pending, FixedTimeProvider.DefaultNow.UtcDateTime);

        [Fact]
        public async Task AddAsync_ThenGetByIdAsync_ShouldReturnOperation()
        {
            var operation = NewOperation();

            await _repository.AddAsync(operation);

            Assert.Same(operation, await _repository.GetByIdAsync(operation.Id));
        }

        [Fact]
        public async Task RemoveAsync_ExistingId_ShouldRemoveOperation()
        {
            var operation = NewOperation();
            await _repository.AddAsync(operation);

            Assert.True(await _repository.RemoveAsync(operation.Id));
            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task RemoveAsync_UnknownId_ShouldReturnFalse() =>
            Assert.False(await _repository.RemoveAsync(Guid.NewGuid()));
    }
}
