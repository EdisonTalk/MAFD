using System.Text.Json.Serialization;

namespace ConditionalEdge.Models;

public class EmailMessage
{
    [JsonPropertyName("email_id")]
    public string EmailId { get; set; } = string.Empty;

    [JsonPropertyName("email_content")]
    public string EmailContent { get; set; } = string.Empty;
}