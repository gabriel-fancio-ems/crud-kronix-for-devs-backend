using System.Text.Json.Serialization;

namespace CrudKronixForDevs.Core.Domains.Enums
{
    /// <summary>
    /// Situação de um centro de operação.
    /// Trafega como texto ("Active", "Inactive"). O atributo garante isso em qualquer serializador,
    /// inclusive no HttpClient dos testes, e não só na configuração da Api.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<OperationCenterStatus>))]
    public enum OperationCenterStatus
    {
        Active = 1,
        Inactive = 2
    }
}
