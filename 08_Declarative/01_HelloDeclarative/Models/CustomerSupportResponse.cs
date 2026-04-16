using System.Text.Json.Serialization;

namespace Declarative_01.Models;

/// <summary>
/// Represents the structured response from the CustomerSupportAgent.
/// The JsonPropertyName attributes map the camelCase properties from the YAML schema
/// to the PascalCase properties in the C# record.
/// </summary>
public sealed record CustomerSupportResponse(
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("sentiment")] string Sentiment,
    [property: JsonPropertyName("answer")] string Answer);
