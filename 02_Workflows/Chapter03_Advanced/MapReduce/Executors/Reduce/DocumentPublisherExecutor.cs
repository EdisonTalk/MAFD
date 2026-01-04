using MapReduce.Events;
using Microsoft.Agents.AI.Workflows;
using System.Text;

namespace MapReduce.Executors.Reduce;

internal sealed class DocumentPublisherExecutor(string id) : Executor<ReduceCompletedEvent>(id)
{
    public override async ValueTask HandleAsync(ReduceCompletedEvent message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();
        builder.AppendLine("《本周安全白皮书摘要》");
        builder.AppendLine(new string('━', 30));
        int order = 1;
        foreach (var summary in message.Summaries)
        {
            builder.AppendLine($"{order++}. {summary}");
        }
        builder.AppendLine(new string('━', 30));
        builder.AppendLine("📌 推荐动作：请检查 OTA 签名、同步 SOC 新规则、与供应链共享告警。");

        await context.YieldOutputAsync(builder.ToString(), cancellationToken);
    }
}