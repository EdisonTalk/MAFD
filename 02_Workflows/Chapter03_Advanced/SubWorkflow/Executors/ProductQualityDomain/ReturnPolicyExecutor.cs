using Microsoft.Agents.AI.Workflows;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.ProductQualityDomain;

// 2. 退换货判定执行器
internal sealed class ReturnPolicyExecutor() : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(ReturnPolicyExecutor))
{
    public override ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("📦 判定退换货政策...");
        var daysFromOrder = (DateTime.Now - record.Original.SubmittedAt).Days;
        if (daysFromOrder <= 7)
        {
            record.ProcessingSteps.Add("[退换货判定] 符合7天无理由退货政策，批准全额退款");
        }
        else
        {
            record.ProcessingSteps.Add("[退换货判定] 超过退货期限，建议换货或部分补偿");
        }
        return ValueTask.FromResult(record);
    }
}