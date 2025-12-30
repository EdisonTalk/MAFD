using Microsoft.Agents.AI.Workflows;
using SwitchCaseV1.Constants;
using SwitchCaseV1.Models;

namespace SwitchCaseV1.Executors;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ⚠️ Handle Uncertain Executor (Default Case)
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
/// <summary>
/// 不确定邮件处理执行器（Default Case）
/// 输入: DetectionResult（应该是 Uncertain，但也处理其他未匹配情况）
/// 输出: 工作流事件
/// </summary>
internal class UncertainHandlingExecutor() : Executor<DetectionResult>("UncertainHandlingExecutor")
{
    public override async ValueTask HandleAsync(
        DetectionResult message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 🛡️ 防御性检查：确保只处理不确定邮件
        if (message.spamDecision != SpamDecision.UnCertain)
            throw new InvalidOperationException(
                "UncertainHandlingExecutor 只应处理 Uncertain 类型的邮件（或作为 Default Case）。"
            );

        // 1️⃣ 从 Shared State 读取原始邮件内容（用于人工审核）
        var email = await context.ReadStateAsync<EmailMessage>(
            message.EmailId,
            scopeName: EmailStateConstants.EmailStateScope,
            cancellationToken
        );

        // 2️⃣ 输出待审核信息
        await context.YieldOutputAsync(
            $"⚠️ 不确定邮件需人工审核:\n" +
            $"原因: {message.Reason}\n" +
            $"内容预览: {email?.EmailContent?.Substring(0, Math.Min(100, email.EmailContent.Length))}...",
            cancellationToken
        );
    }
}