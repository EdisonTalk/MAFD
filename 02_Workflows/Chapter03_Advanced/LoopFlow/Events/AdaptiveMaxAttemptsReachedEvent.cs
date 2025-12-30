using Microsoft.Agents.AI.Workflows;

namespace LoopFlow.Events;

internal sealed class AdaptiveMaxAttemptsReachedEvent : WorkflowEvent
{
    public AdaptiveMaxAttemptsReachedEvent(string ticketId, int maxAttempts)
        : base(new { TicketId = ticketId, MaxAttempts = maxAttempts })
    {
    }
}