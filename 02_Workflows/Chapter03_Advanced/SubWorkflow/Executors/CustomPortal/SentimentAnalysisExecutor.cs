using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using SubWorkflow.Models;

namespace SubWorkflow.Executors.CustomPortal;

// 3. 情绪评估执行器（使用 AI）
internal sealed class SentimentAnalysisExecutor(IChatClient chatClient) : Executor<ComplaintProcessingRecord, ComplaintProcessingRecord>(nameof(SentimentAnalysisExecutor))
{
    public override async ValueTask<ComplaintProcessingRecord> HandleAsync(ComplaintProcessingRecord record, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("😊 AI 情绪评估中...");
        var prompt = $@"分析以下客服回复的情绪基调，用一个词概括（如：友好、专业、冷淡、热情）：

{record.AIGeneratedResponse}

只返回一个词，不要解释。";

        var response = await chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
        record.SentimentScore = response.Text?.Trim() ?? "中性";
        record.ProcessingSteps.Add($"[情绪评估] AI 评估语气为：{record.SentimentScore}");
        return record;
    }
}