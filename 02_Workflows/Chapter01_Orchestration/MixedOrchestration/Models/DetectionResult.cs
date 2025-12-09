using System.Text.Json.Serialization;

namespace MixedOrchestration.Models;

public sealed class DetectionResult
{
    [JsonPropertyName("isJailbreak")]
    public bool IsJailbreak { get; set; }
    [JsonPropertyName("userInput")]
    public string UserInput { get; set; } = string.Empty;
    [JsonPropertyName("detectTime")]
    public DateTime DetectTime { get; set; } = DateTime.Now;
}
