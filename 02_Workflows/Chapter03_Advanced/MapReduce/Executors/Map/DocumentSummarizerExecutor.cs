using MapReduce.Constants;
using MapReduce.Events;
using MapReduce.Models;
using Microsoft.Agents.AI.Workflows;

namespace MapReduce.Executors.Map;

internal sealed class DocumentSummarizerExecutor(string id, string reducerId) : Executor<ChunkReadyEvent>(id)
{
    private readonly string _reducerId = reducerId;

    public override async ValueTask HandleAsync(ChunkReadyEvent message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🟡 {this.Id} 收到 ChunkReadyEvent(order={message.Order})");
        var envelope = await context.ReadStateAsync<ChunkEnvelope>(message.ChunkStateKey, scopeName: DocumentState.Scope, cancellationToken);
        var summary = Summarize(envelope!.Text);
        await context.SendMessageAsync(new ChunkSummaryCompletedEvent(envelope.Order, summary), targetId: this._reducerId, cancellationToken: cancellationToken);
        Console.WriteLine($"📝 {this.Id} 完成段落 {envelope.Order}");
    }

    private static string Summarize(string text)
    {
        var sentences = text.Split(['。', '！', '？'], StringSplitOptions.RemoveEmptyEntries);
        var focus = sentences.Length > 0 ? sentences[0] : text;
        var trimmed = focus.Length > 80 ? focus[..80] + "..." : focus;
        var normalized = trimmed.Replace("\r", string.Empty).Replace("\n", " ").Trim();
        return $"• {normalized}";
    }
}