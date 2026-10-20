using CrudKronixForDevs.Api.Authorization;
using CrudKronixForDevs.Api.Validation;
using CrudKronixForDevs.Core.Applications;
using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Configuration;
using CrudKronixForDevs.Core.Domains.Interfaces;
using CrudKronixForDevs.Core.Dtos;
using CrudKronixForDevs.Core.Dtos.Medications;
using CrudKronixForDevs.Core.Dtos.Users;
using CrudKronixForDevs.Core.Services;
using CrudKronixForDevs.Core.Services.Medications;
using CrudKronixForDevs.Core.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace CrudKronixForDevs.Api.DependencyInjection
{
    /// <summary>
    /// Concentra o registro de dependências para manter o Program.cs curto.
    ///
    /// Tempos de vida no contêiner:
    ///  - Singleton: uma instância para a aplicação inteira.
    ///  - Scoped: uma instância por requisição HTTP.
    ///  - Transient: uma instância nova toda vez que for pedida.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>Tudo o que é específico da camada web.</summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

            services.AddSingleton<IValidator<MedicationRequest>, MedicationRequestValidator>();
            services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();

            return services;
        }

        /// <summary>Autenticação por token JWT, com a chave lida do User Secrets.</summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection(JwtOptions.SectionName))
                .Validate(o => o.Key.Length >= 32,
                    "Jwt:Key não configurada ou com menos de 32 caracteres. " +
                    "Rode na pasta da Api: dotnet user-secrets set \"Jwt:Key\" \"<uma-chave-longa-e-aleatoria>\"")
                .ValidateOnStart();

            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<ITokenService, JwtTokenService>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            // Configura a validação do token a partir do JwtOptions (lido só quando a API sobe).
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    // Mantém os nomes das claims como estão no token ("sub", "email", "name").
                    bearer.MapInboundClaims = false;

                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = JwtTokenService.CreateSigningKey(jwt.Key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        NameClaimType = JwtRegisteredClaimNames.Name
                    };
                });

            services.AddAuthorization();

            return services;
        }

        /// <summary>Tudo o que vem do projeto Core.</summary>
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<UserOptions>()
                .Bind(configuration.GetSection(UserOptions.SectionName))
                .Validate(o => o.IsConfigured(),
                    "Seção User não configurada. Configure o User Secrets do projeto Api " +
                    "(botão direito no projeto > Manage User Secrets) com User:Email, User:Name e User:Password.")
                .ValidateOnStart();

            // Relógio do sistema. Nos testes trocamos por um relógio "parado" numa data fixa.
            services.AddSingleton(TimeProvider.System);

            // Singleton porque são o nosso "banco": precisam sobreviver entre as requisições.
            services.AddSingleton<IMedicationRepository, MedicationRepositoryInMemory>();
            services.AddSingleton<IUserRepository, UserRepositoryInMemory>();

            services.AddScoped<IMedicationApplication, MedicationApplication>();
            services.AddScoped<IAuthApplication, AuthApplication>();

            return services;
        }
    }
}
