namespace LoopFlow.Models.ValueObjects;

internal record TicketOutcome(string TicketId, string Status, int Attempts, QualityReport FinalReport);