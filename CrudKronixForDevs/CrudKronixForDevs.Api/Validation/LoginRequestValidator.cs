using CrudKronixForDevs.Core.Dtos.Users;  
using CrudKronixForDevs.Core.Resources.Users;

namespace CrudKronixForDevs.Api.Validation
{
    /// <summary>Confere se e-mail e senha vieram preenchidos antes de tentar o login.</summary>
    public class LoginRequestValidator : IValidator<LoginRequest>
    {
        public IDictionary<string, string[]> Validate(LoginRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
                errors["email"] = [AuthMessages.InvalidEmail];

            if (string.IsNullOrEmpty(request.Password))
                errors["password"] = [AuthMessages.PasswordRequired];

            return errors;
        }
    }
}
