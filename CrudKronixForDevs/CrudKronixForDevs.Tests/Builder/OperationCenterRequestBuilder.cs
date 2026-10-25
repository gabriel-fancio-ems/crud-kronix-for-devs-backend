using CrudKronixForDevs.Core.Domains.Enums;
using CrudKronixForDevs.Core.Dtos.OperationCenters;

namespace CrudKronixForDevs.Tests.Builder
{
    /// <summary>
    /// Builder de requisições de centro de operação: começa VÁLIDO e cada teste muda só o que interessa.
    /// </summary>
    public class OperationCenterRequestBuilder
    {
        private string? _description = "Centro de Distribuição São Paulo";
        private OperationCenterStatus? _status = OperationCenterStatus.Active;

        public OperationCenterRequestBuilder WithDescription(string? description) { _description = description; return this; }
        public OperationCenterRequestBuilder WithStatus(OperationCenterStatus? status) { _status = status; return this; }

        /// <summary>Descrição única, útil nos testes de integração para não esbarrar na regra de duplicidade.</summary>
        public OperationCenterRequestBuilder WithUniqueDescription() => WithDescription($"Centro {Guid.NewGuid():N}");

        public OperationCenterRequest Build() => new()
        {
            Description = _description,
            Status = _status
        };
    }
}
