using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Resources.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Domains
{
    /// <summary>
    /// Usuário que pode se autenticar na API.
    ///
    /// A senha NUNCA é guardada em texto puro: guardamos apenas o hash dela.
    /// No login, a senha digitada passa pelo mesmo algoritmo e os resultados são comparados.
    /// </summary>
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;

        // Construtor privado: a única forma de criar um usuário é pelo método Create.
        private User() { }

        public static User Create(string name, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException(AuthMessages.NameRequired);
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) throw new DomainException(AuthMessages.InvalidEmail);
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException(AuthMessages.PasswordRequired);

            return new User
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash
            };
        }
    }
}
