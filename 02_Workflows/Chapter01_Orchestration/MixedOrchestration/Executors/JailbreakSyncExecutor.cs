using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace MixedOrchestration.Executors;

/// <summary>
/// Jailbreak 检测结果同步器：解析 Agent 输出，格式化给下一个 Agent
/// </summary>
public sealed class JailbreakSyncExecutor() : Executor<ChatMessage>("JailbreakSync")
{
    public override async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[{Id}] 🔍 解析 Jailbreak 检测结果");

        string fullResponse = message.Text?.Trim() ?? "UNKNOWN";
        Console.WriteLine($"  完整响应: {fullResponse}");

        // 解析检测结果
        bool isJailbreak = fullResponse.Contains("JAILBREAK: DETECTED", StringComparison.OrdinalIgnoreCase) ||
                          fullResponse.Contains("JAILBREAK:DETECTED", StringComparison.OrdinalIgnoreCase);

        Console.WriteLine($"  检测结果: {(isJailbreak ? "❌ 检测到 Jailbreak 攻击" : "✅ 内容安全")}");

        // 提取原始问题
        string originalQuestion = "用户问题";
        int inputIndex = fullResponse.IndexOf("INPUT:", StringComparison.OrdinalIgnoreCase);
        if (inputIndex >= 0)
        {
            originalQuestion = fullResponse.Substring(inputIndex + 6).Trim();
        }

        // 格式化消息给 ResponseAgent
        string formattedMessage = isJailbreak
            ? $"JAILBREAK_DETECTED: 以下问题被标记为不安全: {originalQuestion}"
            : $"SAFE: 请友好地回答这个问题: {originalQuestion}";

        Console.WriteLine($"  格式化消息: {formattedMessage}");
        Console.ResetColor();

        // 发送格式化后的消息给下一个 Agent
        var responseMessage = new ChatMessage(ChatRole.User, formattedMessage);
        await context.SendMessageAsync(responseMessage, cancellationToken: cancellationToken);

        // 发送 TurnToken 触发 ResponseAgent
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken: cancellationToken);
        Console.WriteLine($"  ✅ 已触发 ResponseAgent\n");
    }
}