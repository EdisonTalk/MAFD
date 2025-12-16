using Microsoft.Agents.AI.Workflows;
using SharedState.Data;
using SharedState.Models;

namespace SharedState.Executors;

internal sealed class FileReadExecutor() : Executor<string, string>("FileReadExecutor")
{
    public override async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var content = SharedStateSampleData.GetDocument(message);
        var fileId = Guid.NewGuid().ToString("N");

        await context.QueueStateUpdateAsync(fileId, content, 
            FileContentStateConstants.ScopeName, cancellationToken);
        Console.WriteLine($"📦 FileReadExecutor 已成功将 {message} 写入 Scope:{FileContentStateConstants.ScopeName}");

        return fileId;
    }
}