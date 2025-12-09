using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace MixedOrchestration.Adapters;

/// <summary>
/// Adapter 1: String → ChatMessage + TurnToken
/// 用途：将普通 Executor 的 string 输出转换为 Agent 可接收的格式
/// </summary>
public sealed class StringToChatMessageAdapter(string? id = null) : Executor<string>(id ?? "StringToChatMessage")
{
    public override async ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\n[{Id}] 🔄 类型转换中...");
        Console.WriteLine($"  输入类型: string");
        Console.WriteLine($"  输出类型: ChatMessage + TurnToken");
        Console.WriteLine($"  消息内容: \"{message}\"");
        Console.ResetColor();

        // 步骤 1: 将 string 转换为 ChatMessage
        var chatMessage = new ChatMessage(ChatRole.User, message);
        await context.SendMessageAsync(chatMessage, cancellationToken: cancellationToken);
        Console.WriteLine($"  ✅ 已发送 ChatMessage");

        // 步骤 2: 发送 TurnToken 触发 Agent 执行
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken: cancellationToken);
        Console.WriteLine($"  ✅ 已发送 TurnToken（Agent 将被触发执行）\n");
    }
}