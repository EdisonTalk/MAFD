namespace SubWorkflow.Models;

// 共享状态：投诉处理记录（各执行器会更新此对象）
internal class ComplaintProcessingRecord
{
    public CustomerComplaint Original { get; set; }
    public string Category { get; set; } = "未分类";
    public string Handler { get; set; } = "待分配";
    public List<string> ProcessingSteps { get; set; } = new();
    public string AIGeneratedResponse { get; set; } = "";
    public string ComplianceStatus { get; set; } = "待审核";
    public string SentimentScore { get; set; } = "未评估";
}

// 投诉数据模型
internal record CustomerComplaint(
    string OrderId,
    string CustomerName,
    string ComplaintText,
    DateTime SubmittedAt
);