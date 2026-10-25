using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Services.OperationCenters;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Services
{
    public class OperationCenterRepositoryInMemoryTests
    {
        private readonly OperationCenterRepositoryInMemory _repository = new();

        private static OperationCenter NewOperationCenter(string description = "Centro Principal") =>
            OperationCenter.Add(description, OperationCenterStatus.Active, FixedTimeProvider.DefaultNow.UtcDateTime);

        [Fact]
        public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task AddAsync_ThenGetByIdAsync_ShouldReturnTheSameOperationCenter()
        {
            var operationCenter = NewOperationCenter();

            await _repository.AddAsync(operationCenter);

            Assert.Same(operationCenter, await _repository.GetByIdAsync(operationCenter.Id));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEveryAddedOperationCenter()
        {
            await _repository.AddAsync(NewOperationCenter("A"));
            await _repository.AddAsync(NewOperationCenter("B"));

            Assert.Equal(2, (await _repository.GetAllAsync()).Count);
        }

        [Fact]
        public async Task GetByIdAsync_UnknownId_ShouldReturnNull()
        {
            Assert.Null(await _repository.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateAsync_ShouldKeepASingleRecord()
        {
            var operationCenter = NewOperationCenter();
            await _repository.AddAsync(operationCenter);

            await _repository.UpdateAsync(operationCenter);

            Assert.Single(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task RemoveAsync_ExistingId_ShouldRemoveAndReturnTrue()
        {
            var operationCenter = NewOperationCenter();
            await _repository.AddAsync(operationCenter);

            Assert.True(await _repository.RemoveAsync(operationCenter.Id));
            Assert.Empty(await _repository.GetAllAsync());
        }

        [Fact]
        public async Task RemoveAsync_UnknownId_ShouldReturnFalse()
        {
            Assert.False(await _repository.RemoveAsync(Guid.NewGuid()));
        }
    }
}
