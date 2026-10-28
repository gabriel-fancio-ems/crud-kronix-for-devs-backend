using System.Net;
using System.Net.Http.Json;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Dtos.Operations;
using CrudKronixForDevs.Tests.Builder;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Tests.Integration
{
    public class OperationEndpointsTests(CrudKronixApiFactory factory)
        : IClassFixture<CrudKronixApiFactory>, IAsyncLifetime
    {
        public const string Route = "/api/operation";
        private HttpClient _client = null!;

        public async Task InitializeAsync() => _client = await factory.CreateAuthenticatedClientAsync();

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task FullFlow_CreateGetUpdateDelete_ShouldWork()
        {
            var request = new OperationRequestBuilder().WithUniqueOperation().Build();
            var createResponse = await _client.PostAsJsonAsync(Route, request);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<OperationResponse>();
            Assert.Equal($"{Route}/{created!.Id}", createResponse.Headers.Location!.AbsolutePath, ignoreCase: true);

            var found = await _client.GetFromJsonAsync<OperationResponse>($"{Route}/{created.Id}");
            Assert.Equal(request.Operation, found!.Operation);

            var update = new OperationRequestBuilder()
                .WithProductionOrderId(request.ProductionOrderId)
                .WithWorkCenterId(request.WorkCenterId)
                .WithOperation(request.Operation)
                .WithStatus(OperationStatus.Completed)
                .Build();
            var updateResponse = await _client.PutAsJsonAsync($"{Route}/{created.Id}", update);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<OperationResponse>();
            Assert.Equal(OperationStatus.Completed, updated!.Status);

            var deleteResponse = await _client.DeleteAsync($"{Route}/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"{Route}/{created.Id}")).StatusCode);
        }

        [Fact]
        public async Task Create_WithInvalidData_ShouldReturn400WithAllErrors()
        {
            var response = await _client.PostAsJsonAsync(Route, new OperationRequest());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Contains("productionOrderId", problem!.Errors.Keys);
            Assert.Contains("operation", problem.Errors.Keys);
            Assert.Contains("workCenterId", problem.Errors.Keys);
            Assert.Contains("status", problem.Errors.Keys);
        }

        [Fact]
        public async Task Create_ShouldSerializeStatusAsText()
        {
            var response = await _client.PostAsJsonAsync(Route, new OperationRequestBuilder()
                .WithStatus(OperationStatus.InProgress)
                .Build());

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"status\":\"InProgress\"", json);
        }

        [Fact]
        public async Task List_WithFilter_ShouldFindCreatedOperation()
        {
            var request = new OperationRequestBuilder().WithUniqueOperation().Build();
            var createResponse = await _client.PostAsJsonAsync(Route, request);
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<OperationResponse>();

            var list = await _client.GetFromJsonAsync<List<OperationResponse>>(
                $"{Route}?operation={Uri.EscapeDataString(request.Operation!)}");

            Assert.Equal(created!.Id, Assert.Single(list!).Id);
        }

        [Fact]
        public async Task GetById_UnknownId_ShouldReturn404()
        {
            var response = await _client.GetAsync($"{Route}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
