namespace CrudKronixForDevs.Api.Authorization
{
    /// <summary>
    /// Configuração do token JWT (seção "Jwt").
    /// Issuer, Audience e ExpirationMinutes ficam no appsettings.json.
    /// A Key é segredo: fica no User Secrets (desenvolvimento) ou na variável de ambiente Jwt__Key.
    /// </summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        /// <summary>Chave de assinatura. Mínimo de 32 caracteres para o algoritmo HS256.</summary>
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = "CrudKronixForDevs";
        public string Audience { get; set; } = "CrudKronixForDevs";

        /// <summary>Por quantos minutos o token vale depois do login.</summary>
        public int ExpirationMinutes { get; set; } = 60;
    }

}
