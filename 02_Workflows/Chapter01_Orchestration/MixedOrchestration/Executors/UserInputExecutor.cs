using Microsoft.Agents.AI.Workflows;

namespace MixedOrchestration.Executors;

/// <summary>
/// 用户输入执行器：接收并存储用户问题
/// </summary>
public sealed class UserInputExecutor() : Executor<string, string>("UserInput")
{
    public override async ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[{Id}] 📝 接收用户输入");
        Console.WriteLine($"  问题: \"{message}\"");
        Console.ResetColor();

        // 将原始问题存储到工作流状态中，供后续使用
        await context.QueueStateUpdateAsync("OriginalQuestion", message, cancellationToken);
        Console.WriteLine($"  ✅ 已存储到工作流状态 (Key: OriginalQuestion)\n");

        return message;
    }
}