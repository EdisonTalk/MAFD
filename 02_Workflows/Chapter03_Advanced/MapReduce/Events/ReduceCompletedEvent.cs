using Microsoft.Agents.AI.Workflows;

namespace MapReduce.Events;

internal sealed class ReduceCompletedEvent(IReadOnlyList<string> Summaries) : WorkflowEvent
{
    public IReadOnlyList<string> Summaries { get; } = Summaries;
}