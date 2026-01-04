using Microsoft.Agents.AI.Workflows;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.ProductLogisticsDomain;

// 1. 物流追踪执行器
internal sealed class LogisticsTrackingExecutor() : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(LogisticsTrackingExecutor))
{
    public override ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🚚 正在查询物流信息...");
        record.ProcessingSteps.Add("[物流追踪] 包裹在中转站滞留3天，当前状态：运输中");
        record.Handler = "物流运营团队";
        return ValueTask.FromResult(record);
    }
}