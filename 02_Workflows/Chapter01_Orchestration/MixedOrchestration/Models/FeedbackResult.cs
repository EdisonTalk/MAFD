using System.Text.Json.Serialization;

namespace MixedOrchestration.Models;

/// <summary>
/// 审核反馈结果
/// </summary>
public sealed class FeedbackResult
{
    /// <summary>
    /// 审核评论
    /// </summary>
    [JsonPropertyName("comments")]
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// 质量评分（1-10分）
    /// </summary>
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    /// <summary>
    /// 改进建议
    /// </summary>
    [JsonPropertyName("actions")]
    public string Actions { get; set; } = string.Empty;
}