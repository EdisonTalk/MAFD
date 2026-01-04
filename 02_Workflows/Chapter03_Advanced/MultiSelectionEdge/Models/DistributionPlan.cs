namespace MultiSelectionEdge.Models;

internal sealed class DistributionPlan
{
    public required string SubmissionId { get; init; }
    public bool PublishToLongForm { get; init; }
    public bool PublishToShortForm { get; init; }
    public bool EscalateToModerator { get; init; }
    public string Reason { get; init; } = string.Empty;
}