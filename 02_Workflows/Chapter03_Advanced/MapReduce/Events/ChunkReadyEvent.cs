using Microsoft.Agents.AI.Workflows;

namespace MapReduce.Events;

internal sealed class ChunkReadyEvent(string ChunkStateKey, int Order) : WorkflowEvent
{
    public string ChunkStateKey { get; } = ChunkStateKey;
    public int Order { get; } = Order;
}