using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Domains.Interfaces;
using CrudKronixForDevs.Core.Dtos.OperationCenters;
using CrudKronixForDevs.Core.Extensions;
using CrudKronixForDevs.Core.Resources.OperationCenters;

namespace CrudKronixForDevs.Core.Applications
{
    public class OperationCenterApplication(
        IOperationCenterRepository repository,
        IMedicationRepository medicationRepository,
        TimeProvider timeProvider) : IOperationCenterApplication
    {
        public async Task<IReadOnlyList<OperationCenterResponse>> ListAsync(string? description)
        {
            var operationCenters = await repository.GetAllAsync();
            var filter = description.NormalizeForSearch();

            return operationCenters
                .Where(o => filter.Length == 0 || o.Description.NormalizeForSearch().Contains(filter))
                .OrderBy(o => o.Description)
                .Select(o => o.ToResponse())
                .ToList();
        }

        public async Task<OperationCenterResponse?> GetByIdAsync(Guid id)
        {
            var operationCenter = await repository.GetByIdAsync(id);
            return operationCenter?.ToResponse();
        }

        public async Task<OperationCenterResponse> CreateAsync(OperationCenterRequest request)
        {
            await EnsureNotDuplicatedAsync(request, idIgnored: null);

            var operationCenter = OperationCenter.Add(
                request.Description ?? string.Empty,
                request.Status ?? default,
                Now());

            await repository.AddAsync(operationCenter);
            return operationCenter.ToResponse();
        }

        public async Task<OperationCenterResponse?> UpdateAsync(Guid id, OperationCenterRequest request)
        {
            var operationCenter = await repository.GetByIdAsync(id);
            if (operationCenter is null) return null;

            await EnsureNotDuplicatedAsync(request, idIgnored: id);

            operationCenter.Update(
                request.Description ?? string.Empty,
                request.Status ?? default);

            await repository.UpdateAsync(operationCenter);
            return operationCenter.ToResponse();
        }

        /// <summary>
        /// Regra: um centro de operação com medicamentos vinculados não pode ser removido,
        /// senão esses medicamentos ficariam apontando para um centro que não existe.
        /// </summary>
        public async Task<bool> RemoveAsync(Guid id)
        {
            var operationCenter = await repository.GetByIdAsync(id);
            if (operationCenter is null) return false;

            var medications = await medicationRepository.GetAllAsync();
            if (medications.Any(m => m.OperationCenterId == id))
                throw new OperationCenterInUseException(
                    OperationCenterMessages.OperationCenterInUse(operationCenter.Description));

            return await repository.RemoveAsync(id);
        }

        /// <summary>
        /// Regra: não podem existir dois centros de operação com a mesma descrição.
        /// </summary>
        private async Task EnsureNotDuplicatedAsync(OperationCenterRequest request, Guid? idIgnored)
        {
            var descricao = request.Description.NormalizeForSearch();

            var existentes = await repository.GetAllAsync();

            var duplicado = existentes.Any(o =>
                o.Id != idIgnored
                && o.Description.NormalizeForSearch() == descricao);

            if (duplicado)
                throw new DuplicateOperationCenterException(
                    OperationCenterMessages.DuplicateOperationCenter(request.Description!));
        }

        private DateTime Now() => timeProvider.GetLocalNow().DateTime;
    }
}
