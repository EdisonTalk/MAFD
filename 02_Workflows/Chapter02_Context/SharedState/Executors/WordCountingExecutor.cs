using Microsoft.Agents.AI.Workflows;
using SharedState.Models;

namespace SharedState.Executors;

internal sealed class WordCountingExecutor() : Executor<string, FileStats>("WordCountingExecutor")
{
    public override async ValueTask<FileStats> HandleAsync(string fileId, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string? content = await context.ReadStateAsync<string>(fileId, FileContentStateConstants.ScopeName, cancellationToken);
        if (content is null)
        {
            throw new InvalidOperationException($"无法在 Scope:{FileContentStateConstants.ScopeName} 中找到 fileId={fileId}");
        }

        int wordCount = content.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        return new FileStats { WordCount = wordCount };
    }
}