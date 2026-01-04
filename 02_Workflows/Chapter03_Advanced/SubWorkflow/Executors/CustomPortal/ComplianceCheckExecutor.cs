using Microsoft.Agents.AI.Workflows;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.CustomPortal;

// 2. 合规审核执行器
internal sealed class ComplianceCheckExecutor() : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(ComplianceCheckExecutor))
{
    public override ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🛡️ 合规审核中...");
        var checks = new List<string>();
        if (record.AIGeneratedResponse.Contains("歉意")) checks.Add("✅ 包含道歉");
        if (record.AIGeneratedResponse.Length <= 300) checks.Add("✅ 字数合规");
        if (!record.AIGeneratedResponse.Contains("法律") && !record.AIGeneratedResponse.Contains("诉讼"))
            checks.Add("✅ 无敏感法律词汇");

        record.ComplianceStatus = checks.Count >= 2 ? "✅ 审核通过" : "⚠️ 需人工复审";
        record.ProcessingSteps.Add($"[合规审核] {record.ComplianceStatus} - {string.Join(", ", checks)}");
        return ValueTask.FromResult(record);
    }
}