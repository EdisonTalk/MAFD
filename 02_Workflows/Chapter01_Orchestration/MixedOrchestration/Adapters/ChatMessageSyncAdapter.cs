using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace MixedOrchestration.Adapters;

/// <summary>
/// Adapter 2: ChatMessage 同步处理器
/// 用途：处理 Agent 输出，格式化后传递给下一个 Agent
/// </summary>
public sealed class ChatMessageSyncAdapter(string id) : Executor<ChatMessage>(id)
{
    public override async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n[{Id}] 🔄 Agent 输出同步中...");
        Console.WriteLine($"  收到消息: {message.Text}");
        Console.ResetColor();

        // 这里可以对 Agent 的输出进行处理、解析、格式化等
        // 例如：提取关键信息、检查输出格式、添加上下文等

        string processedText = message.Text ?? "";

        // 示例：将处理后的内容重新包装成 ChatMessage
        var newMessage = new ChatMessage(ChatRole.User, processedText);
        await context.SendMessageAsync(newMessage, cancellationToken: cancellationToken);
        Console.WriteLine($"  ✅ 已发送处理后的 ChatMessage");

        // 发送 TurnToken 触发下一个 Agent
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken: cancellationToken);
        Console.WriteLine($"  ✅ 已发送 TurnToken（下一个 Agent 将被触发）\n");
    }
}