using CrudKronixForDevs.Api.DependencyInjection;
using CrudKronixForDevs.Api.Middlewares;
using Scalar.AspNetCore;

// 1) Configuração: registra no contêiner de injeção de dependência tudo o que a aplicação usa.
//    Em Development, o CreateBuilder já lê o User Secrets (por causa do UserSecretsId no .csproj).
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddCoreServices(builder.Configuration);

var app = builder.Build();

// 2) Pipeline: a ordem importa! Cada requisição passa por estes passos de cima para baixo.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Mapeia a interface do Scalar na rota /scalar
    app.MapScalarApiReference(options =>
    {
        options.Title = "Minha API Documentada"; // Atribuição direta de propriedade
        options.WithTheme(ScalarTheme.Kepler);
        options.DarkMode = true;
    });
}

app.UseAuthentication();   // lê o token e descobre QUEM está chamando
app.UseAuthorization();    // decide se essa pessoa PODE acessar o endpoint

app.MapControllers();

await app.AdminUserAsync();

app.Run();

// Necessário para os testes de integração enxergarem a classe Program.
public partial class Program { }
