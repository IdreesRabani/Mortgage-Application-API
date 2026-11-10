using System.Text.Json.Serialization;

namespace Mortage_Application_API.Domain.Affordability
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AffordabilityDecision
    {
        Approved,
        Declined
    }

}
