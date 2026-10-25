using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Dtos.Medications
{
    /// <summary>
    /// DTO de ENTRADA: o JSON que o cliente envia no POST e no PUT.
    ///
    /// Os campos são anuláveis (string?, DateOnly?) para que possamos devolver
    /// mensagens de validação claras em português quando algo não for enviado.
    /// </summary>
    public class MedicationRequest
    {
        /// <example>Dipirona Monoidratada</example>
        public string? Name { get; init; }

        /// <example>Dipirona</example>
        public string? ActiveIngredient { get; init; }

        /// <example>12.90</example>
        public decimal Price { get; init; }

        /// <example>150</example>
        public int StockQuantity { get; init; }

        /// <example>2026-10-20</example>
        public DateOnly? ExpirationDate { get; init; }

        /// <summary>Id de um centro de operação já cadastrado.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid? OperationCenterId { get; init; }
    }
}
