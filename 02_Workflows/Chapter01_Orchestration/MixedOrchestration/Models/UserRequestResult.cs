using System.Text.Json.Serialization;

namespace MixedOrchestration.Models;

public sealed class UserRequestResult
{
    [JsonPropertyName("userInput")]
    public string UserInput { get; set; } = string.Empty;
    [JsonPropertyName("finalResponse")]
    public string FinalResponse { get; set; } = string.Empty;
    [JsonPropertyName("respondTime")]
    public DateTime RespondTime { get; set; } = DateTime.Now;
}
