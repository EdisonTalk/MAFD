using MapReduce.Constants;
using MapReduce.Events;
using Microsoft.Agents.AI.Workflows;

namespace MapReduce.Executors.Reduce;

internal sealed class ConsensusReducerExecutor(string id, string publisherId) : Executor<ChunkSummaryCompletedEvent>(id)
{
    private readonly SortedDictionary<int, string> _summaries = new();
    private readonly string _publisherId = publisherId;
    private int? _expectedChunks;

    public override async ValueTask HandleAsync(ChunkSummaryCompletedEvent message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        _expectedChunks ??= await context.ReadStateAsync<int>(DocumentState.TotalChunksKey, scopeName: DocumentState.Scope, cancellationToken);
        _summaries[message.Order] = message.Summary;
        Console.WriteLine($"📊 Reduce 进度: {_summaries.Count}/{_expectedChunks}");

        if (_expectedChunks.HasValue && _summaries.Count >= _expectedChunks.Value)
        {
            var ordered = _summaries.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
            await context.SendMessageAsync(new ReduceCompletedEvent(ordered), targetId: this._publisherId, cancellationToken: cancellationToken);
            Console.WriteLine("✨ Reduce 阶段完成");
        }
    }
}