using SwitchCaseV1.Models;
using Microsoft.Agents.AI.Workflows;

namespace SwitchCaseV1.Executors;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📤 Send Email Executor
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

/// <summary>
/// 邮件发送执行器（模拟发送）
/// 输入: EmailResponse
/// 输出: 工作流事件
/// </summary>
internal sealed class EmailSendingExecutor() : Executor<EmailResponse>("EmailSendingExecutor")
{
    public override async ValueTask HandleAsync(EmailResponse message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // 模拟邮件发送（实际项目中可调用 SMTP、SendGrid 等服务）
        await context.YieldOutputAsync(
            $"📤 邮件已发送: {message.Response}",
            cancellationToken
        );
    }
}