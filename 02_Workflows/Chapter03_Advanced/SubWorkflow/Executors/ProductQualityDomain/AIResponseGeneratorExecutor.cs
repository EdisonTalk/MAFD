using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.ProductQualityDomain;

// 3. AI生成回复执行器
internal sealed class AIResponseGeneratorExecutor(IChatClient chatClient) : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(AIResponseGeneratorExecutor))
{
    public override async ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🤖 AI 正在生成客户回复...");
        var prompt = $@"你是专业的客服主管。根据以下投诉处理信息，生成一封正式、有同理心的客户回复邮件（150字内）：

客户：{record.Original.CustomerName}
订单号：{record.Original.OrderId}
投诉内容：{record.Original.ComplaintText}
处理步骤：
{string.Join("\n", record.ProcessingSteps)}

要求：
1. 表达歉意和理解
2. 说明处理方案
3. 提供后续联系方式
4. 语气真诚、专业";

        var response = await chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
        record.AIGeneratedResponse = response.Text ?? "AI 生成失败";
        record.ProcessingSteps.Add($"[AI 回复] 已生成客户回复模板（{record.AIGeneratedResponse.Length}字）");
        return record;
    }
}