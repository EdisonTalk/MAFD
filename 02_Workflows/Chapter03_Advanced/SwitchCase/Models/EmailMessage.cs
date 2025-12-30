using System.Text.Json.Serialization;

namespace SwitchCaseV1.Models;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📧 邮件内容数据模型
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
/// <summary>
/// 邮件内容（存储在 Shared State 中）
/// </summary>
public class EmailMessage
{
    [JsonPropertyName("email_id")]
    public string EmailId { get; set; } = string.Empty;
    [JsonPropertyName("email_content")]
    public string EmailContent { get; set; } = string.Empty;
    [JsonPropertyName("receive_time")]
    public DateTime ReceivedAt { get; set; } = DateTime.Now;
}