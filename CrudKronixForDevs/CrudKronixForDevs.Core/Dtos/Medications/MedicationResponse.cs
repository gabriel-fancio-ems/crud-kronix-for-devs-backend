using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Dtos.Medications
{
    /// <summary>
    /// DTO de SAÍDA: o JSON que a API devolve.
    /// Nunca devolvemos a entidade de domínio diretamente; assim podemos mudar a entidade
    /// sem quebrar quem consome a API.
    /// </summary>
    public record MedicationResponse(
        Guid Id,
        string Name,
        string ActiveIngredient,
        decimal Price,
        int StockQuantity,
        DateOnly ExpirationDate,
        Guid OperationCenterId,
        DateTime CreatedAt);
}
