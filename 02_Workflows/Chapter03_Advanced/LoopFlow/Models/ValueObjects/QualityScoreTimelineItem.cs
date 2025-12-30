namespace LoopFlow.Models.ValueObjects;

internal sealed record QualityScoreTimelineItem(
    int Attempt,
    int PolitenessScore,
    int AccuracyScore,
    string ComplianceStatus);