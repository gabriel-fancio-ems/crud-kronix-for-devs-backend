namespace CrudKronixForDevs.Core.Resources.OperationCenters
{
    /// <summary>
    /// Centraliza os textos de centros de operação exibidos ao usuário.
    /// </summary>
    public static class OperationCenterMessages
    {
        public const string DescriptionRequired = "A descrição do centro de operação é obrigatória.";
        public const string StatusRequired = "O status do centro de operação é obrigatório.";
        public const string StatusInvalid = "O status informado é inválido. Valores aceitos: Active, Inactive.";
        public const string OperationCenterRequired = "O centro de operação é obrigatório.";
        public const string OperationCenterNotFound = "O centro de operação informado não existe.";

        public static string MaximumSize(string campo, int maximo) =>
            $"O campo {campo} deve ter no máximo {maximo} caracteres.";

        public static string DuplicateOperationCenter(string descricao) =>
            $"Já existe o centro de operação '{descricao}' cadastrado no sistema.";

        public static string OperationCenterInUse(string descricao) =>
            $"O centro de operação '{descricao}' possui medicamentos vinculados e não pode ser removido.";
    }
}
