using CrudKronixForDevs.Core.Dtos.OperationCenters;
using CrudKronixForDevs.Core.Resources.OperationCenters;

namespace CrudKronixForDevs.Api.Validation
{
    public class OperationCenterRequestValidator : IValidator<OperationCenterRequest>
    {
        private const int SizeDescription = 150;

        public IDictionary<string, string[]> Validate(OperationCenterRequest request)
        {
            var erros = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(request.Description))
                Add(erros, "descricao", OperationCenterMessages.DescriptionRequired);
            else if (request.Description.Trim().Length > SizeDescription)
                Add(erros, "descricao", OperationCenterMessages.MaximumSize("descricao", SizeDescription));

            if (request.Status is null)
                Add(erros, "status", OperationCenterMessages.StatusRequired);
            else if (!Enum.IsDefined(request.Status.Value))
                Add(erros, "status", OperationCenterMessages.StatusInvalid);

            return erros.ToDictionary(e => e.Key, e => e.Value.ToArray());
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
