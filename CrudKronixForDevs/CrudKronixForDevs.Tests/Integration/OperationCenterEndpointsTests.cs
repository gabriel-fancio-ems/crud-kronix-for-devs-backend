using System.Net;
using System.Net.Http.Json;
using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Dtos.Medications;
using CrudKronixForDevs.Core.Dtos.OperationCenters;
using CrudKronixForDevs.Tests.Builder;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Tests.Integration
{
    /// <summary>
    /// Testes de ponta a ponta do CRUD de centros de operação (/api/operation-center).
    /// O "banco" em memória é compartilhado, então cada teste usa descrições únicas.
    /// </summary>
    public class OperationCenterEndpointsTests(CrudKronixApiFactory factory)
        : IClassFixture<CrudKronixApiFactory>, IAsyncLifetime
    {
        public const string Route = "/api/operation-center";

        private HttpClient _client = null!;

        public async Task InitializeAsync() => _client = await factory.CreateAuthenticatedClientAsync();

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        private static OperationCenterRequestBuilder NewRequest() => new OperationCenterRequestBuilder().WithUniqueDescription();

        /// <summary>
        /// Cadastra um centro de operação com descrição única. Também usado pelos testes de medicamentos,
        /// que precisam de um centro existente.
        /// </summary>
        public static async Task<OperationCenterResponse> CreateOperationCenterAsync(HttpClient client, OperationCenterRequest? request = null)
        {
            var response = await client.PostAsJsonAsync(Route, request ?? NewRequest().Build());
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<OperationCenterResponse>())!;
        }

        // ---------------------------------------------------------------- Fluxo completo

        [Fact]
        public async Task FullFlow_CreateGetUpdateDelete_ShouldWork()
        {
            var request = NewRequest().Build();
            var createResponse = await _client.PostAsJsonAsync(Route, request);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<OperationCenterResponse>();
            Assert.Equal($"{Route}/{created!.Id}", createResponse.Headers.Location!.AbsolutePath, ignoreCase: true);

            var found = await _client.GetFromJsonAsync<OperationCenterResponse>($"{Route}/{created.Id}");
            Assert.Equal(request.Description, found!.Description);

            var update = new OperationCenterRequestBuilder()
                .WithDescription(request.Description)
                .WithStatus(OperationCenterStatus.Inactive)
                .Build();
            var updateResponse = await _client.PutAsJsonAsync($"{Route}/{created.Id}", update);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<OperationCenterResponse>();
            Assert.Equal(OperationCenterStatus.Inactive, updated!.Status);

            var deleteResponse = await _client.DeleteAsync($"{Route}/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var afterDelete = await _client.GetAsync($"{Route}/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        }

        // ---------------------------------------------------------------- POST

        [Fact]
        public async Task Create_ShouldSerializeStatusAsText()
        {
            var response = await _client.PostAsJsonAsync(Route, NewRequest().Build());

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"status\":\"Active\"", json);
        }

        [Fact]
        public async Task Create_WithInvalidData_ShouldReturn400WithAllErrors()
        {
            var response = await _client.PostAsJsonAsync(Route, new OperationCenterRequest());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Contains("descricao", problem!.Errors.Keys);
            Assert.Contains("status", problem.Errors.Keys);
        }

        [Fact]
        public async Task Create_Duplicated_ShouldReturn409()
        {
            var request = NewRequest().Build();
            await CreateOperationCenterAsync(_client, request);

            var response = await _client.PostAsJsonAsync(Route, request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal("Centro de operação duplicado", problem!.Title);
        }

        // ---------------------------------------------------------------- GET

        [Fact]
        public async Task List_WithFilter_ShouldFindCreatedOperationCenter()
        {
            var created = await CreateOperationCenterAsync(_client);

            var list = await _client.GetFromJsonAsync<List<OperationCenterResponse>>(
                $"{Route}?descricao={Uri.EscapeDataString(created.Description)}");

            Assert.Equal(created.Id, Assert.Single(list!).Id);
        }

        [Fact]
        public async Task GetById_UnknownId_ShouldReturn404()
        {
            var response = await _client.GetAsync($"{Route}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------------------------------------------------------------- PUT / DELETE

        [Fact]
        public async Task Update_UnknownId_ShouldReturn404()
        {
            var response = await _client.PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", NewRequest().Build());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_UnknownId_ShouldReturn404()
        {
            var response = await _client.DeleteAsync($"{Route}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithLinkedMedication_ShouldReturn409()
        {
            var operationCenter = await CreateOperationCenterAsync(_client);
            var medication = new MedicationRequestBuilder().WithUniqueName().WithOperationCenterId(operationCenter.Id).Build();
            (await _client.PostAsJsonAsync("/api/medication", medication)).EnsureSuccessStatusCode();

            var response = await _client.DeleteAsync($"{Route}/{operationCenter.Id}");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal("Centro de operação em uso", problem!.Title);
        }

        // ---------------------------------------------------------------- Segurança

        public static TheoryData<string, string> ProtectedEndpoints => new()
        {
            { "GET", Route },
            { "GET", $"{Route}/{Guid.Empty}" },
            { "POST", Route },
            { "PUT", $"{Route}/{Guid.Empty}" },
            { "DELETE", $"{Route}/{Guid.Empty}" }
        };

        [Theory]
        [MemberData(nameof(ProtectedEndpoints))]
        public async Task AnyEndpoint_WithoutToken_ShouldReturn401(string method, string url)
        {
            using var anonymous = factory.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (method is "POST" or "PUT")
                request.Content = JsonContent.Create(NewRequest().Build());

            var response = await anonymous.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
