using CrudKronixForDevs.Core.Applications.Interfaces;
using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Domains.Exceptions;
using CrudKronixForDevs.Core.Domains.Interfaces;
using CrudKronixForDevs.Core.Dtos.Medications;
using CrudKronixForDevs.Core.Extensions;
using CrudKronixForDevs.Core.Resources.Medications;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Applications
{
    public class MedicationApplication(
        IMedicationRepository repository,
        TimeProvider timeProvider) : IMedicationApplication
    {
        public async Task<IReadOnlyList<MedicationResponse>> ListAsync(string? name)
        {
            var medication = await repository.GetAllAsync();
            var filter = name.NormalizeForSearch();

            return medication
                .Where(m => filter.Length == 0
                            || m.Name.NormalizeForSearch().Contains(filter)
                            || m.ActiveIngredient.NormalizeForSearch().Contains(filter))
                .OrderBy(m => m.Name)
                .Select(m => m.ToResponse())
                .ToList();
        }

        public async Task<MedicationResponse?> GetByIdAsync(Guid id)
        {
            var medication = await repository.GetByIdAsync(id);
            return medication?.ToResponse();
        }

        public async Task<IReadOnlyList<MedicationResponse>> ListItemsNearExpiration(int days)
        {
            var now = DateOnly.FromDateTime(Now());
            var medications = await repository.GetAllAsync();

            return medications
                .OrderBy(m => m.ExpirationDate)
                .Select(m => m.ToResponse())
                .ToList();
        }

        public async Task<MedicationResponse> CreateAsync(MedicationRequest request)
        {
            await EnsureNotDuplicatedAsync(request, idIgnored: null);

            var medication = Medication.Add(
                request.Name ?? string.Empty,
                request.ActiveIngredient ?? string.Empty,
                request.Price,
                request.QuantityInStock,
                request.ExpirationDate ?? default,
                Now());

            await repository.AddAsync(medication);
            return medication.ToResponse();
        }

        public async Task<MedicationResponse?> UpdateAsync(Guid id, MedicationRequest request)
        {
            var medication = await repository.GetByIdAsync(id);
            if (medication is null) return null;

            await EnsureNotDuplicatedAsync(request, idIgnored: id);

            medication.Update(
                request.Name ?? string.Empty,
                request.ActiveIngredient ?? string.Empty,
                request.Price,
                request.QuantityInStock,
                request.ExpirationDate ?? default,
                Now());

            await repository.UpdateAsync(medication);
            return medication.ToResponse();
        }

        public Task<bool> RemoveAsync(Guid id) => repository.RemoveAsync(id);

        /// <summary>
        /// Regra: não podem existir dois medicamentos com mesmo nome.
        /// Ela fica aqui (e não na entidade) porque precisa consultar os outros medicamentos.
        /// </summary>
        private async Task EnsureNotDuplicatedAsync(MedicationRequest request, Guid? idIgnored)
        {
            var nome = request.Name.NormalizeForSearch();

            var existentes = await repository.GetAllAsync();

            var duplicado = existentes.Any(m =>
                m.Id != idIgnored
                && m.Name.NormalizeForSearch() == nome);

            if (duplicado)
                throw new DuplicateMedicationException(
                    MedicationMessages.DuplicateMedication(request.Name!));
        }

        private DateTime Now() => timeProvider.GetLocalNow().DateTime;
    }
}
