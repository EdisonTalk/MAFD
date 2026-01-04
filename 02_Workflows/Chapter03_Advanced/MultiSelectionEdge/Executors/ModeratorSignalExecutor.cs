using Microsoft.Agents.AI.Workflows;
using MultiSelectionEdge.Models;

namespace MultiSelectionEdge.Executors;

internal sealed class ModeratorSignalExecutor() : Executor<DistributionPlan>(nameof(ModeratorSignalExecutor))
{
    public override async ValueTask HandleAsync(DistributionPlan plan, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await context.YieldOutputAsync($"🛡️ {plan.SubmissionId} 触发人工审核：{plan.Reason}", cancellationToken);
    }
}