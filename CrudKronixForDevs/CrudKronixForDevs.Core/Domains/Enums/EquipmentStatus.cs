using System.Text.Json.Serialization;

namespace CrudKronixForDevs.Core.Domains.Enums
{
    /// <summary>
    /// Situação de um equipamento.
    /// Trafega como texto ("Active", "Inactive"). O atributo garante isso em qualquer serializador,
    /// inclusive no HttpClient dos testes, e não só na configuração da Api.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<EquipmentStatus>))]
    public enum EquipmentStatus
    {
        Active = 1,
        Inactive = 2
    }
}
