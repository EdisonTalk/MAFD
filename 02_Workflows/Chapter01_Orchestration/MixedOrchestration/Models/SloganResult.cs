using System.Text.Json.Serialization;

namespace MixedOrchestration.Models;
/// <summary>
/// 文案生成结果
/// </summary>
public sealed class SloganResult
{
    /// <summary>
    /// 产品任务描述
    /// </summary>
    [JsonPropertyName("task")]
    public required string Task { get; set; }

    /// <summary>
    /// 生成的标语
    /// </summary>
    [JsonPropertyName("slogan")]
    public required string Slogan { get; set; }
}