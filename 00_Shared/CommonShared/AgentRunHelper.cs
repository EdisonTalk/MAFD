using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace CommonShared;

public static class AgentRunHelper
{
    public static async IAsyncEnumerable<T> OfEventType<T>(
         IAsyncEnumerable<WorkflowEvent> events) where T : WorkflowEvent
    {
        await foreach (var evt in events)
        {
            if (evt is T typedEvent)
            {
                yield return typedEvent;
            }
        }
    }

    public static async Task<List<ChatMessage>> RunWorkflowStreamingAsync(Workflow workflow, string message)
    {
        List<ChatMessage> results = new();

        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, message);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is ExecutorInvokedEvent invoked)
            {
                Console.WriteLine($"🚀 Executor invoked with ID: {invoked.ExecutorId}");
            }

            if (evt is WorkflowErrorEvent error)
            {
                Console.WriteLine();
                Console.WriteLine($"✅ Workflow completed with error: {error.Data}");
            }

            if (evt is WorkflowOutputEvent output)
            {
                results = output.As<List<ChatMessage>>()!;
            }
        }
        await run.DisposeAsync();

        return results;
    }
}