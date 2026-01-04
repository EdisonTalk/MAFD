using Microsoft.Agents.AI.Workflows;
using MultiSelectionEdge.Models;

namespace MultiSelectionEdge.Executors;

internal sealed class LongFormChannelExecutor() : Executor<DistributionPlan>(nameof(LongFormChannelExecutor))
{
    public override async ValueTask HandleAsync(DistributionPlan plan, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await context.YieldOutputAsync($"📰 " + plan.SubmissionId + " 发布到长文渠道", cancellationToken);
    }
}