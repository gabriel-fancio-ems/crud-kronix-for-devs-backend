namespace CrudKronixForDevs.Core.Resources.Operations
{
    public static class OperationMessages
    {
        public const string ProductionOrderRequired = "A ordem de produção é obrigatória.";
        public const string NameRequired = "A operação é obrigatória.";
        public const string NameTooLong = "A operação deve ter no máximo 150 caracteres.";
        public const string WorkCenterRequired = "O centro de trabalho é obrigatório.";
        public const string StatusRequired = "O status da operação é obrigatório.";
        public const string StatusInvalid = "O status informado é inválido. Valores aceitos: Pending, InProgress, Completed.";
    }
}
