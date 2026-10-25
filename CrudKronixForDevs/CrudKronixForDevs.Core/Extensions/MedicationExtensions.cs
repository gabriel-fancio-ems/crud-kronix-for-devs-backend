using CrudKronixForDevs.Core.Domains;
using CrudKronixForDevs.Core.Dtos.Medications;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Extensions
{
    /// <summary>
    /// Métodos de extensão para converter a entidade em DTO (o famoso "mapeamento").
    /// Um método de extensão permite escrever medication.ToResponse() como se o método
    /// fosse da própria classe Medication.
    /// </summary>
    public static class MedicationExtensions
    {
        public static MedicationResponse ToResponse(this Medication medication) => new(
            medication.Id,
            medication.Name,
            medication.ActiveIngredient,
            medication.Price,
            medication.StockQuantity,
            medication.ExpirationDate,
            medication.OperationCenterId,
            medication.CreatedAt
        );
    }
}
