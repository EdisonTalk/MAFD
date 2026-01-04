using MapReduce.Constants;
using MapReduce.Events;
using MapReduce.Models;
using Microsoft.Agents.AI.Workflows;

namespace MapReduce.Executors.Map;

internal sealed class ChunkSplitterExecutor(string[] summarizerIds) : Executor<string>(nameof(ChunkSplitterExecutor))
{
    private readonly string[] _summarizerIds = summarizerIds;

    public override async ValueTask HandleAsync(string manuscript, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var paragraphs = manuscript
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        await context.QueueStateUpdateAsync(DocumentState.TotalChunksKey, paragraphs.Length, scopeName: DocumentState.Scope, cancellationToken);
        Console.WriteLine($"🧩 Map 阶段：段落数 = {paragraphs.Length}");

        for (int i = 0; i < paragraphs.Length; i++)
        {
            var targetId = this._summarizerIds[i % this._summarizerIds.Length];
            var chunkStateKey = $"chunk_{i}"; // 简化键，避免ID耦合
            var envelope = new ChunkEnvelope(chunkStateKey, paragraphs[i], i);

            await context.QueueStateUpdateAsync(chunkStateKey, envelope, scopeName: DocumentState.Scope, cancellationToken);
            Console.WriteLine($"➡️ 发送 ChunkReadyEvent(order={i}) 到 {targetId}");
            await context.SendMessageAsync(new ChunkReadyEvent(chunkStateKey, i), targetId: targetId, cancellationToken: cancellationToken);
        }

        Console.WriteLine($"🧩 Map 阶段：已拆分 {paragraphs.Length} 个段落");
    }
}