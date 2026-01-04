using Microsoft.Agents.AI.Workflows;
using MultiSelectionEdge.Models;

namespace MultiSelectionEdge.Executors;

internal sealed class ContentClassifierExecutor() : Executor<ContentSubmission, DistributionPlan>(nameof(ContentClassifierExecutor))
{
    public override ValueTask<DistributionPlan> HandleAsync(ContentSubmission submission, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        bool publishLong = submission.Length > 600;
        bool publishShort = submission.Length <= 600 || submission.Category is "Marketing";
        bool needModerator = submission.ContainsRisk || submission.Category is "Compliance";

        return ValueTask.FromResult(new DistributionPlan
        {
            SubmissionId = submission.Id,
            PublishToLongForm = publishLong,
            PublishToShortForm = publishShort,
            EscalateToModerator = needModerator,
            Reason = needModerator ? 
                "命中风险或合规模块" : "常规稿件"
        });
    }
}