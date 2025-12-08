using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace MixedOrchestration.Adapters;

/// <summary>
/// Adapter 3: ChatMessage → String
/// 用途：将 Agent 输出转换为普通 Executor 可处理的 string 类型
/// </summary>
public sealed class ChatMessageToStringAdapter(string id) : Executor<ChatMessage, string>(id)
{
    public override ValueTask<string> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[{Id}] 🔄 类型转换中...");
        Console.WriteLine($"  输入类型: ChatMessage");
        Console.WriteLine($"  输出类型: string");
        Console.ResetColor();

        string result = message.Text ?? "";
        Console.WriteLine($"  ✅ 提取文本内容: \"{result}\"\n");

        return ValueTask.FromResult(result);
    }
}
