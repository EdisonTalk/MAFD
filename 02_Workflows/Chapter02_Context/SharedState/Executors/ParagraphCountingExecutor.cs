using Microsoft.Agents.AI.Workflows;
using SharedState.Models;

namespace SharedState.Executors;

internal sealed class ParagraphCountingExecutor() : Executor<string, FileStats>("ParagraphCountingExecutor")
{
    public override async ValueTask<FileStats> HandleAsync(string fileId, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string? content = await context.ReadStateAsync<string>(fileId, FileContentStateConstants.ScopeName, cancellationToken);
        if (content is null)
        {
            throw new InvalidOperationException($"无法在 Scope:{FileContentStateConstants.ScopeName} 中找到 fileId={fileId}");
        }

        int paragraphCount = content.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        return new FileStats { ParagraphCount = paragraphCount };
    }
}