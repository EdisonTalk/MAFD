using Microsoft.Agents.AI.Workflows;
using MultiSelectionEdge.Models;

namespace MultiSelectionEdge.Executors;

internal sealed class ShortFormChannelExecutor() : Executor<DistributionPlan>(nameof(ShortFormChannelExecutor))
{
    public override async ValueTask HandleAsync(DistributionPlan plan, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await context.YieldOutputAsync($"📣 " + plan.SubmissionId + " 发布到短文渠道", cancellationToken);
    }
}