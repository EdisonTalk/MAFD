using Microsoft.Agents.AI.Workflows;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.ProductQualityDomain;

// 1. 问题评估执行器
internal sealed class ProductEvaluationExecutor() : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(ProductEvaluationExecutor))
{
    public override ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🔍 正在评估产品质量问题...");
        record.ProcessingSteps.Add("[产品评估] 检测到屏幕外观缺陷，符合质量问题定义");
        record.Handler = "产品质量团队";
        return ValueTask.FromResult(record);
    }
}