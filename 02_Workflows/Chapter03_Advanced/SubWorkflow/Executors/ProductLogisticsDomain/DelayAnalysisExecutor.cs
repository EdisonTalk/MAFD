using Microsoft.Agents.AI.Workflows;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.ProductLogisticsDomain;

// 2. 延迟分析执行器
internal sealed class DelayAnalysisExecutor() : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(DelayAnalysisExecutor))
{
    public override ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("📊 分析延迟原因...");
        record.ProcessingSteps.Add("[延迟分析] 延迟原因：暴雨导致道路封闭，预计2天内恢复配送");
        record.ProcessingSteps.Add("[补偿方案] 提供50元优惠券 + 免运费");
        return ValueTask.FromResult(record);
    }
}