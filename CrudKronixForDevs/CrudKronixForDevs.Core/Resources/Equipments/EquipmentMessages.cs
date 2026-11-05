namespace CrudKronixForDevs.Core.Resources.Equipments
{
    /// <summary>
    /// Centraliza os textos de equipamentos exibidos ao usuário.
    /// </summary>
    public static class EquipmentMessages
    {
        public const string CodeRequired = "O código do equipamento é obrigatório.";
        public const string DescriptionRequired = "A descrição do equipamento é obrigatória.";
        public const string StatusRequired = "O status do equipamento é obrigatório.";
        public const string StatusInvalid = "O status informado é inválido. Valores aceitos: Active, Inactive.";

        public static string MaximumSize(string campo, int maximo) =>
            $"O campo {campo} deve ter no máximo {maximo} caracteres.";

        public static string DuplicateEquipment(string codigo) =>
            $"Já existe o equipamento com código '{codigo}' cadastrado no sistema.";
    }
}
