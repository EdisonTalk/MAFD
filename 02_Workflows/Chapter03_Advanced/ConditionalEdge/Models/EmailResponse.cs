using System.Text.Json.Serialization;

namespace ConditionalEdge.Models;

public sealed class EmailResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}