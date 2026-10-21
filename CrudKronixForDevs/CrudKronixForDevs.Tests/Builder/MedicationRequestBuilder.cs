using CrudKronixForDevs.Core.Dtos.Medications;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Tests.Builder
{
    /// <summary>
    /// Padrão "Builder" para montar requisições nos testes.
    ///
    /// Ele começa com um medicamento VÁLIDO e cada teste muda só o que interessa:
    ///
    ///   var request = new MedicationRequestBuilder().WithPrice(0).Build();
    ///
    /// Assim o teste deixa claro qual dado está sendo testado, sem repetir os outros campos.
    /// </summary>
    public class MedicationRequestBuilder
    {
        private string? _name = "Dipirona Monoidratada";
        private string? _activeIngredient = "Dipirona";
        private decimal _price = 12.90m;
        private int _stockQuantity = 150;
        private DateOnly? _expirationDate;

        /// <param name="today">Data de "hoje" usada para calcular uma validade futura (padrão: daqui a 1 ano).</param>
        public MedicationRequestBuilder(DateOnly? today = null)
        {
            _expirationDate = (today ?? DateOnly.FromDateTime(DateTime.Today)).AddYears(1);
        }

        public MedicationRequestBuilder WithName(string? name) { _name = name; return this; }
        public MedicationRequestBuilder WithActiveIngredient(string? activeIngredient) { _activeIngredient = activeIngredient; return this; }
        public MedicationRequestBuilder WithPrice(decimal price) { _price = price; return this; }
        public MedicationRequestBuilder WithStockQuantity(int stockQuantity) { _stockQuantity = stockQuantity; return this; }
        public MedicationRequestBuilder WithExpirationDate(DateOnly? expirationDate) { _expirationDate = expirationDate; return this; }

        /// <summary>Nome único, útil nos testes de integração para não esbarrar na regra de duplicidade.</summary>
        public MedicationRequestBuilder WithUniqueName() => WithName($"Medicamento {Guid.NewGuid():N}");

        public MedicationRequest Build() => new()
        {
            Name = _name,
            ActiveIngredient = _activeIngredient,
            Price = _price,
            StockQuantity = _stockQuantity,
            ExpirationDate = _expirationDate
        };
    }

}
