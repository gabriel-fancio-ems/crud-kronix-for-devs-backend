using CrudKronixForDevs.Core.Dtos.Operations;
using CrudKronixForDevs.Core.Resources.Operations;

namespace CrudKronixForDevs.Api.Validation
{
    public class OperationRequestValidator : IValidator<OperationRequest>
    {
        private const int SizeOperation = 150;

        public IDictionary<string, string[]> Validate(OperationRequest request)
        {
            var errors = new Dictionary<string, List<string>>();

            if (request.ProductionOrderId is null || request.ProductionOrderId == Guid.Empty)
                Add(errors, "productionOrderId", OperationMessages.ProductionOrderRequired);

            if (string.IsNullOrWhiteSpace(request.Operation))
                Add(errors, "operation", OperationMessages.NameRequired);
            else if (request.Operation.Trim().Length > SizeOperation)
                Add(errors, "operation", OperationMessages.NameTooLong);

            if (request.WorkCenterId is null || request.WorkCenterId == Guid.Empty)
                Add(errors, "workCenterId", OperationMessages.WorkCenterRequired);

            if (request.Status is null)
                Add(errors, "status", OperationMessages.StatusRequired);
            else if (!Enum.IsDefined(request.Status.Value))
                Add(errors, "status", OperationMessages.StatusInvalid);

            return errors.ToDictionary(error => error.Key, error => error.Value.ToArray());
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
