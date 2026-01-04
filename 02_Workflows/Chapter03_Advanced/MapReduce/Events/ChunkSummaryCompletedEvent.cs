using Microsoft.Agents.AI.Workflows;

namespace MapReduce.Events;

internal sealed class ChunkSummaryCompletedEvent(int Order, string Summary) : WorkflowEvent
{
    public int Order { get; } = Order;
    public string Summary { get; } = Summary;
}