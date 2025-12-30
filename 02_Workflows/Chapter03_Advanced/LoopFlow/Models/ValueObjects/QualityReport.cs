namespace LoopFlow.Models.ValueObjects;

internal record QualityReport(string TicketId, int PolitenessScore, int AccuracyScore, bool CompliancePassed, IReadOnlyList<QualityIssue> Issues);