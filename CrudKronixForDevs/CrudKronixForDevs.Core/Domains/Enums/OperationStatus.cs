using System.Text.Json.Serialization;

namespace CrudKronixForDevs.Core.Domains.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<OperationStatus>))]
    public enum OperationStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3
    }
}
