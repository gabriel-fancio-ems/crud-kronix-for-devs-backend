using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrudKronixForDevs.Core.Resources.Medications
{
    /// <summary>
    /// Centraliza os textos exibidos ao usuário.
    /// Manter as mensagens em um único lugar evita textos repetidos espalhados pelo código
    /// </summary>
    public static class MedicationMessages
    {
        public const string NameRequired = "O nome do medicamento é obrigatório.";
        public const string ActiveIngredientRequired = "O princípio ativo é obrigatório.";
        public const string ManufacturerRequired = "O fabricante é obrigatório.";
        public const string PriceInvalid = "O preço deve ser maior que zero.";
        public const string StockInvalid = "A quantidade em estoque não pode ser negativa.";
        public const string ExpirationDateRequired = "A data de validade é obrigatória.";
        public const string ExpirationDateMustBeFuture = "A data de validade deve ser posterior à data de hoje.";

        public static string MaximumSize(string campo, int maximo) =>
            $"O campo {campo} deve ter no máximo {maximo} caracteres.";

        public static string DuplicateMedication(string nome) =>
            $"Já existe o medicamento '{nome}' cadastrado no sistema.";
    }
}
