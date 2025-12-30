namespace LoopFlow.Models.ValueObjects;

internal record QualityIssue(string Type, string Description, int ScoreImpact);