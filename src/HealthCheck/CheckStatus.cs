using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HealthChecks
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CheckStatus
    {
        Unknown,
        Unhealthy,
        Healthy,
        Warning
    }
}
