using CrudKronixForDevs.Core.Dtos.Medications;
using CrudKronixForDevs.Core.Resources.Medications;
using CrudKronixForDevs.Core.Resources.OperationCenters;

namespace CrudKronixForDevs.Api.Validation
{
    public class MedicationRequestValidator : IValidator<MedicationRequest>
    {
        private const int SizeName = 150;
        private const int SizeActiveIngredient = 150;
        private const decimal MaxPrice = 100_000m;

        public IDictionary<string, string[]> Validate(MedicationRequest request)
        {
            var erros = new Dictionary<string, List<string>>();

            ValidateText(erros, "nome", request.Name, SizeName, MedicationMessages.NameRequired);
            ValidateText(erros, "principioAtivo", request.ActiveIngredient, SizeActiveIngredient, MedicationMessages.ActiveIngredientRequired);

            if (request.Price <= 0)
                Add(erros, "preco", MedicationMessages.PriceInvalid);
            else if (request.Price > MaxPrice)
                Add(erros, "preco", $"O preço deve ser no máximo {MaxPrice:N2}.");

            if (request.StockQuantity < 0)
                Add(erros, "quantidadeEstoque", MedicationMessages.StockInvalid);

            if (request.ExpirationDate is null)
                Add(erros, "dataValidade", MedicationMessages.ExpirationDateRequired);

            // Aqui só conferimos se o id foi enviado; se o centro existe, quem confere é o caso de uso.
            if (request.OperationCenterId is null || request.OperationCenterId == Guid.Empty)
                Add(erros, "centroOperacao", OperationCenterMessages.OperationCenterRequired);

            return erros.ToDictionary(e => e.Key, e => e.Value.ToArray());
        }

        private static void ValidateText(Dictionary<string, List<string>> errors, string field, string? value, int sizeMax, string messageRequired)
        {
            if (string.IsNullOrWhiteSpace(value))
                Add(errors, field, messageRequired);
            else if (value.Trim().Length > sizeMax)
                Add(errors, field, MedicationMessages.MaximumSize(field, sizeMax));
        }

        private static void Add(Dictionary<string, List<string>> errors, string field, string message)
        {
            if (!errors.TryGetValue(field, out var list))
            {
                list = [];
                errors[field] = list;
            }

            list.Add(message);
        }
    }
}
