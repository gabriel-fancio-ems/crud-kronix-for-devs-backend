using System.Net;
using System.Net.Http.Json;
using CrudKronixForDevs.Core.Dtos.Medications;
using CrudKronixForDevs.Tests.Builder;
using Microsoft.AspNetCore.Mvc;

namespace CrudKronixForDevs.Tests.Integration
{
    /// <summary>
    /// Testes de ponta a ponta do CRUD de medicamentos (/api/medication).
    ///
    /// IAsyncLifetime: o xUnit chama InitializeAsync antes de cada teste. Usamos isso
    /// para fazer login e já ter um HttpClient autenticado.
    ///
    /// Atenção: o "banco" em memória é compartilhado entre os testes desta classe.
    /// Por isso cada teste usa nomes únicos (WithUniqueName) e não conta quantos itens existem no total.
    /// </summary>
    public class MedicationEndpointsTests(CrudKronixApiFactory factory)
        : IClassFixture<CrudKronixApiFactory>, IAsyncLifetime
    {
        private const string Route = "/api/medication";

        private HttpClient _client = null!;

        public async Task InitializeAsync() => _client = await factory.CreateAuthenticatedClientAsync();

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        private static MedicationRequestBuilder NewRequest() => new MedicationRequestBuilder().WithUniqueName();

        // Cadastra um medicamento e devolve o que a API respondeu.
        private async Task<MedicationResponse> CreateAsync(MedicationRequest request)
        {
            var response = await _client.PostAsJsonAsync(Route, request);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<MedicationResponse>())!;
        }

        // ---------------------------------------------------------------- Fluxo completo

        [Fact]
        public async Task FullFlow_CreateGetUpdateDelete_ShouldWork()
        {
            // 1) Criar → 201 Created + cabeçalho Location
            var request = NewRequest().WithPrice(10m).Build();
            var createResponse = await _client.PostAsJsonAsync(Route, request);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<MedicationResponse>();
            Assert.NotNull(created);
            Assert.Equal($"{Route}/{created.Id}", createResponse.Headers.Location!.AbsolutePath, ignoreCase: true);

            // 2) Buscar pelo id → 200
            var found = await _client.GetFromJsonAsync<MedicationResponse>($"{Route}/{created.Id}");
            Assert.Equal(request.Name, found!.Name);

            // 3) Atualizar → 200 com os novos dados
            var update = new MedicationRequestBuilder().WithName(request.Name).WithPrice(99.90m).Build();
            var updateResponse = await _client.PutAsJsonAsync($"{Route}/{created.Id}", update);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<MedicationResponse>();
            Assert.Equal(99.90m, updated!.Price);

            // 4) Remover → 204, e depois não existe mais → 404
            var deleteResponse = await _client.DeleteAsync($"{Route}/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var afterDelete = await _client.GetAsync($"{Route}/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        }

        // ---------------------------------------------------------------- POST

        [Fact]
        public async Task Create_ShouldReturnAllFieldsInTheResponse()
        {
            var request = NewRequest()
                .WithActiveIngredient("Paracetamol")
                .WithPrice(18.90m)
                .WithQuantityInStock(120)
                .Build();

            var created = await CreateAsync(request);

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal(request.Name, created.Name);
            Assert.Equal("Paracetamol", created.ActiveIngredient);
            Assert.Equal(18.90m, created.Price);
            Assert.Equal(120, created.StockQuantity);
            Assert.Equal(request.ExpirationDate, created.ExpirationDate);
        }

        [Fact]
        public async Task Create_WithInvalidData_ShouldReturn400WithAllErrors()
        {
            var response = await _client.PostAsJsonAsync(Route, new MedicationRequest { Price = 0, QuantityInStock = -5 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Contains("nome", problem!.Errors.Keys);
            Assert.Contains("principioAtivo", problem.Errors.Keys);
            Assert.Contains("preco", problem.Errors.Keys);
            Assert.Contains("quantidadeEstoque", problem.Errors.Keys);
            Assert.Contains("dataValidade", problem.Errors.Keys);
        }

        [Fact]
        public async Task Create_WithPastExpirationDate_ShouldReturn400FromBusinessRule()
        {
            // O validador aceita (a data foi enviada); quem barra é a entidade → middleware → 400.
            var request = NewRequest().WithExpirationDate(DateOnly.FromDateTime(DateTime.Today).AddDays(-1)).Build();

            var response = await _client.PostAsJsonAsync(Route, request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal("Regra de negócio violada", problem!.Title);
        }

        [Fact]
        public async Task Create_Duplicated_ShouldReturn409()
        {
            var request = NewRequest().Build();
            await CreateAsync(request);

            var response = await _client.PostAsJsonAsync(Route, request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal("Medicamento duplicado", problem!.Title);
        }

        // ---------------------------------------------------------------- GET

        [Fact]
        public async Task List_WithFilter_ShouldFindCreatedMedication()
        {
            var created = await CreateAsync(NewRequest().Build());

            var list = await _client.GetFromJsonAsync<List<MedicationResponse>>(
                $"{Route}?nome={Uri.EscapeDataString(created.Name)}");

            Assert.Equal(created.Id, Assert.Single(list!).Id);
        }

        [Fact]
        public async Task List_WithoutFilter_ShouldIncludeCreatedMedication()
        {
            var created = await CreateAsync(NewRequest().Build());

            var list = await _client.GetFromJsonAsync<List<MedicationResponse>>(Route);

            Assert.Contains(list!, m => m.Id == created.Id);
        }

        [Fact]
        public async Task GetById_UnknownId_ShouldReturn404()
        {
            var response = await _client.GetAsync($"{Route}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_InvalidGuid_ShouldReturn404()
        {
            // A rota exige {id:guid}; "abc" não casa com nenhuma rota.
            var response = await _client.GetAsync($"{Route}/abc");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------------------------------------------------------------- PUT

        [Fact]
        public async Task Update_UnknownId_ShouldReturn404()
        {
            var response = await _client.PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", NewRequest().Build());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithInvalidData_ShouldReturn400()
        {
            var created = await CreateAsync(NewRequest().Build());

            var response = await _client.PutAsJsonAsync($"{Route}/{created.Id}", new MedicationRequest());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_UsingNameOfAnotherMedication_ShouldReturn409()
        {
            var first = await CreateAsync(NewRequest().Build());
            var second = await CreateAsync(NewRequest().Build());

            var response = await _client.PutAsJsonAsync($"{Route}/{second.Id}",
                new MedicationRequestBuilder().WithName(first.Name).Build());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // ---------------------------------------------------------------- DELETE

        [Fact]
        public async Task Delete_UnknownId_ShouldReturn404()
        {
            var response = await _client.DeleteAsync($"{Route}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ---------------------------------------------------------------- Segurança

        // Cada linha é um endpoint protegido: método HTTP + caminho.
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
            using var anonymous = factory.CreateClient(); // cliente SEM o cabeçalho Authorization
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (method is "POST" or "PUT")
                request.Content = JsonContent.Create(NewRequest().Build());

            var response = await anonymous.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
