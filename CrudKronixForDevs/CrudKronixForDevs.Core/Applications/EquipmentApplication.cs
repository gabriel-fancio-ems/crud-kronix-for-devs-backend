using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Domains.Interfaces;
using CrudKronixForDevs.Core.Dtos.Equipments;
using CrudKronixForDevs.Core.Extensions;
using CrudKronixForDevs.Core.Resources.Equipments;

namespace CrudKronixForDevs.Core.Applications
{
    public class EquipmentApplication(
        IEquipmentRepository repository,
        TimeProvider timeProvider) : IEquipmentApplication
    {
        public async Task<IReadOnlyList<EquipmentResponse>> ListAsync()
        {
            var equipments = await repository.GetAllAsync();

            return equipments
                .OrderBy(e => e.Code)
                .Select(e => e.ToResponse())
                .ToList();
        }

        public async Task<EquipmentResponse> CreateAsync(EquipmentRequest request)
        {
            await EnsureNotDuplicatedAsync(request, idIgnored: null);

            var equipment = Equipment.Add(
                request.Code ?? string.Empty,
                request.Description ?? string.Empty,
                request.Status ?? default,
                Now());

            await repository.AddAsync(equipment);
            return equipment.ToResponse();
        }


        public async Task<EquipmentResponse?> GetByIdAsync(Guid id)
        {
            var equipment = await repository.GetByIdAsync(id);
            return equipment?.ToResponse();
        }

         public async Task<EquipmentResponse?> UpdateAsync(Guid id, EquipmentRequest request)
        {
            var equipment = await repository.GetByIdAsync(id);
            if (equipment is null) return null;

            await EnsureNotDuplicatedAsync(request, idIgnored: id);

            equipment.Update(
                request.Code ?? string.Empty,
                request.Description ?? string.Empty,
                request.Status ?? default);

            await repository.UpdateAsync(equipment);
            return equipment.ToResponse();
        }



        /// <summary>
        /// Regra: não podem existir dois equipamentos com o mesmo código.
        /// </summary>
        private async Task EnsureNotDuplicatedAsync(EquipmentRequest request, Guid? idIgnored)
        {
            var code = request.Code.NormalizeForSearch();
            var descricao = request.Description.NormalizeForSearch();

            var existentes = await repository.GetAllAsync();

            var duplicado = existentes.Any(e => e.Id != idIgnored && e.Code.NormalizeForSearch() == code && e.Description.NormalizeForSearch() == descricao);

            if (duplicado)
                throw new DuplicateEquipmentException(EquipmentMessages.DuplicateEquipment(request.Description!.Trim()));
        }

        private DateTime Now() => timeProvider.GetLocalNow().DateTime;
    }
}
