using ConditionalEdge.Models;
using Microsoft.Agents.AI.Workflows;

namespace ConditionalEdge.Executors;

internal sealed class SpamHandlingExecutor() : Executor<DetectionResult>("SpamHandlingExecutor")
{
    public override async ValueTask HandleAsync(DetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (!message.IsSpam)
            throw new InvalidOperationException("非垃圾邮件不应进入 SpamHandlingExecutor。");

        await context.YieldOutputAsync($"🚨 垃圾邮件：{message.Reason}", cancellationToken);
    }
}