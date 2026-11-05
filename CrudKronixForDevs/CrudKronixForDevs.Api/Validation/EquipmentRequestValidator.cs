using CrudKronixForDevs.Core.Dtos.Equipments;
using CrudKronixForDevs.Core.Resources.Equipments;

namespace CrudKronixForDevs.Api.Validation
{
    public class EquipmentRequestValidator : IValidator<EquipmentRequest>
    {
        private const int SizeCode = 50;
        private const int SizeDescription = 150;

        public IDictionary<string, string[]> Validate(EquipmentRequest request)
        {
            var erros = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(request.Code))
                Add(erros, "codigo", EquipmentMessages.CodeRequired);
            else if (request.Code.Trim().Length > SizeCode)
                Add(erros, "codigo", EquipmentMessages.MaximumSize("codigo", SizeCode));

            if (string.IsNullOrWhiteSpace(request.Description))
                Add(erros, "descricao", EquipmentMessages.DescriptionRequired);
            else if (request.Description.Trim().Length > SizeDescription)
                Add(erros, "descricao", EquipmentMessages.MaximumSize("descricao", SizeDescription));

            if (request.Status is null)
                Add(erros, "status", EquipmentMessages.StatusRequired);
            else if (!Enum.IsDefined(request.Status.Value))
                Add(erros, "status", EquipmentMessages.StatusInvalid);

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
