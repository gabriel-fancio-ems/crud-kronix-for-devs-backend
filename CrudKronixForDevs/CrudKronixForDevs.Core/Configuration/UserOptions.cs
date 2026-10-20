using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Configuration
{
    /// <summary>
    /// Usuário administrador criado na inicialização da API.
    ///
    /// IMPORTANTE: estes valores NÃO ficam no appsettings.json (que vai para o Git).
    /// Em desenvolvimento eles vêm do User Secrets (secrets.json, fora da pasta do projeto).
    /// Em produção ou Docker, use variáveis de ambiente: User__Email, User__Name, User__Password.
    /// </summary>
    public class UserOptions
    {
        public const string SectionName = "User";

        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>Indica se os três campos foram preenchidos.</summary>
        public bool IsConfigured() =>
            !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Password);
    }
}
