using Microsoft.Agents.AI.Workflows;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.CustomPortal;

// 1. 投诉分类执行器
internal sealed class ComplaintClassifierExecutor() : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(ComplaintClassifierExecutor))
{
    public override ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🏷️ 正在分类投诉类型...");
        // 简单规则分类（实际可用 AI 分类）
        if (record.Original.ComplaintText.Contains("划痕") || record.Original.ComplaintText.Contains("质量") || record.Original.ComplaintText.Contains("退货"))
        {
            record.Category = "产品质量";
        }
        else if (record.Original.ComplaintText.Contains("延迟") || record.Original.ComplaintText.Contains("物流") || record.Original.ComplaintText.Contains("配送"))
        {
            record.Category = "物流问题";
        }
        else
        {
            record.Category = "其他";
        }
        record.ProcessingSteps.Add($"[分类器] 投诉类型识别为：{record.Category}");
        return ValueTask.FromResult(record);
    }
}