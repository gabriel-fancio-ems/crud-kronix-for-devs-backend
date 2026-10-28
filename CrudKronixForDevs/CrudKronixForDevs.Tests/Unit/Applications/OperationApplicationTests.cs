using CrudKronixForDevs.Core.Applications;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Services.Operations;
using CrudKronixForDevs.Tests.Builder;
using CrudKronixForDevs.Tests.Fakes;

namespace CrudKronixForDevs.Tests.Unit.Applications
{
    public class OperationApplicationTests
    {
        private readonly OperationRepositoryInMemory _repository = new();
        private readonly FixedTimeProvider _clock = new();
        private readonly OperationApplication _application;

        public OperationApplicationTests() => _application = new OperationApplication(_repository, _clock);

        [Fact]
        public async Task CreateAsync_ShouldSaveAndReturnOperation()
        {
            var request = new OperationRequestBuilder().Build();

            var created = await _application.CreateAsync(request);

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal(request.ProductionOrderId, created.ProductionOrderId);
            Assert.Equal(request.Operation, created.Operation);
            Assert.Equal(request.WorkCenterId, created.WorkCenterId);
            Assert.Equal(request.Status, created.Status);
            Assert.Equal(_clock.GetUtcNow().UtcDateTime, created.CreatedAt);
            Assert.NotNull(await _repository.GetByIdAsync(created.Id));
        }

        [Fact]
        public async Task ListAsync_ShouldFilterByOperationAndIgnoreCaseAndAccents()
        {
            await _application.CreateAsync(new OperationRequestBuilder().WithOperation("Inspeção final").Build());
            await _application.CreateAsync(new OperationRequestBuilder().WithOperation("Montagem").Build());

            var result = await _application.ListAsync("INSPECAO");

            Assert.Equal("Inspeção final", Assert.Single(result).Operation);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingOperation()
        {
            var created = await _application.CreateAsync(new OperationRequestBuilder().Build());
            var updated = await _application.UpdateAsync(created.Id,
                new OperationRequestBuilder().WithOperation("Inspeção").WithStatus(OperationStatus.Completed).Build());

            Assert.NotNull(updated);
            Assert.Equal("Inspeção", updated.Operation);
            Assert.Equal(OperationStatus.Completed, updated.Status);
            Assert.Equal(created.CreatedAt, updated.CreatedAt);
        }

        [Fact]
        public async Task UpdateAsync_UnknownId_ShouldReturnNull() =>
            Assert.Null(await _application.UpdateAsync(Guid.NewGuid(), new OperationRequestBuilder().Build()));

        [Fact]
        public async Task RemoveAsync_ShouldRemoveExistingOperation()
        {
            var created = await _application.CreateAsync(new OperationRequestBuilder().Build());

            Assert.True(await _application.RemoveAsync(created.Id));
            Assert.Null(await _application.GetByIdAsync(created.Id));
        }
    }
}
