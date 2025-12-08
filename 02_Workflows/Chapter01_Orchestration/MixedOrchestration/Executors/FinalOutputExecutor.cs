using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace MixedOrchestration.Executors;

/// <summary>
/// 最终输出执行器：展示工作流的最终结果
/// </summary>
public sealed class FinalOutputExecutor() : Executor<ChatMessage, string>("FinalOutput")
{
    public override ValueTask<string> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n{'━',60}");
        Console.WriteLine($"[{Id}] 📤 最终回复");
        Console.WriteLine($"{'━',60}");
        Console.WriteLine(message.Text);
        Console.WriteLine($"{'━',60}");
        Console.WriteLine($"\n✅ 工作流执行完成\n");
        Console.ResetColor();

        return ValueTask.FromResult(message.Text ?? string.Empty);
    }
}