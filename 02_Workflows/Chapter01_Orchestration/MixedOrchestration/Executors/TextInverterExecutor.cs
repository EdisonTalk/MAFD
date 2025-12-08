using Microsoft.Agents.AI.Workflows;

namespace MixedOrchestration.Executors;

/// <summary>
/// 文本倒序执行器：演示数据处理（实际业务中可能是数据清洗、验证等）
/// </summary>
public sealed class TextInverterExecutor(string id) : Executor<string, string>(id)
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string inverted = string.Concat(message.Reverse());

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{Id}] 🔄 文本倒序处理");
        Console.WriteLine($"  原文: {message}");
        Console.WriteLine($"  结果: {inverted}\n");
        Console.ResetColor();

        return ValueTask.FromResult(inverted);
    }
}