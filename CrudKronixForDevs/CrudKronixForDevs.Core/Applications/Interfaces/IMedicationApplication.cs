using CrudKronixForDevs.Core.Dtos.Medications;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Applications.Interfaces
{
    /// <summary>
    /// Casos de uso de medicamentos: tudo o que o sistema sabe FAZER com eles.
    /// O Controller conversa apenas com esta interface.
    /// </summary>
    public interface IMedicationApplication
    {
        Task<IReadOnlyList<MedicationResponse>> ListAsync(string? nome);
        Task<MedicationResponse?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<MedicationResponse>> ListItemsNearExpiration(int dias);
        Task<MedicationResponse> CreateAsync(MedicationRequest request);

        /// <returns>O medicamento atualizado, ou null se o id não existir.</returns>
        Task<MedicationResponse?> UpdateAsync(Guid id, MedicationRequest request);

        /// <returns>true se removeu; false se o id não existir.</returns>
        Task<bool> RemoveAsync(Guid id);
    }
}
