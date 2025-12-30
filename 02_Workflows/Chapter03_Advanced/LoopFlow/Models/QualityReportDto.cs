using System.Text.Json.Serialization;

namespace LoopFlow.Models;
// 质检结果的结构化输出模型
internal class QualityReportDto
{
    [JsonPropertyName("politenessScore")]
    public int PolitenessScore { get; set; }

    [JsonPropertyName("accuracyScore")]
    public int AccuracyScore { get; set; }

    [JsonPropertyName("compliancePassed")]
    public bool CompliancePassed { get; set; }

    [JsonPropertyName("issues")]
    public List<QualityIssueDto> Issues { get; set; } = new();
}

internal class QualityIssueDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("scoreImpact")]
    public int ScoreImpact { get; set; }
}