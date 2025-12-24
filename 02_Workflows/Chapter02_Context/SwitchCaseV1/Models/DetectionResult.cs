using System.Text.Json.Serialization;

namespace SwitchCaseV1.Models;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📦 检测结果数据模型
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
public sealed class DetectionResult
{
    /// <summary>
    /// 检测决策（NotSpam / Spam / Uncertain）
    /// </summary>
    [JsonPropertyName("spam_decision")]
    [JsonConverter(typeof(JsonStringEnumConverter))]  // JSON 序列化为字符串
    public SpamDecision spamDecision { get; set; }
    /// <summary>
    /// 判定理由（用于审计和调试）
    /// </summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
    /// <summary>
    /// 邮件ID（用于关联 Shared State 中的原始内容）
    /// </summary>
    [JsonIgnore]
    public string EmailId { get; set; } = string.Empty;
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 🎯 垃圾邮件判定枚举（三分类）
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
public enum SpamDecision
{
    Spam,        // 垃圾邮件
    NotSpam, // 正常邮件
    UnCertain // 无法确定（需要人工审核）
}