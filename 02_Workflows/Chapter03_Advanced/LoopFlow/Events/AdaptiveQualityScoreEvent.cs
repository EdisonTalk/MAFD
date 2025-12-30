using Microsoft.Agents.AI.Workflows;

namespace LoopFlow.Events;

internal sealed class AdaptiveQualityScoreEvent : WorkflowEvent
{
    public AdaptiveQualityScoreEvent(string ticketId, 
        int attempt, 
        int politenessScore, 
        int accuracyScore, 
        bool compliancePassed)
        : base(new { TicketId = ticketId, Attempt = attempt, PolitenessScore = politenessScore, AccuracyScore = accuracyScore, CompliancePassed = compliancePassed })
    {
    }
}