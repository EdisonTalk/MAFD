using Microsoft.Agents.AI.Workflows;

namespace LoopFlow.Events;

internal sealed class LoopProgressEvent : WorkflowEvent
{
    public LoopProgressEvent(string ticketId, 
        int attempt, 
        int politenessScore, 
        int accuracyScore, 
        bool compliancePassed, 
        string stage)
        : base(new { ticketId, attempt, politenessScore, accuracyScore, compliancePassed, stage })
    {
    }
}