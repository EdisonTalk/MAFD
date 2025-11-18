using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BasicAgent.Models;

[Description("Information about a person including their name, age, and occupation")]
public class PersonInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}