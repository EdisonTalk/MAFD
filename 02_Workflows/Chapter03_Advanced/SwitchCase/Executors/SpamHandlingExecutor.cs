using SwitchCaseV1.Models;
using Microsoft.Agents.AI.Workflows;

namespace SwitchCaseV1.Executors;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 🚫 Handle Spam Executor
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
/// <summary>
/// 垃圾邮件处理执行器
/// 输入: DetectionResult（必须是 Spam）
/// 输出: 工作流事件
/// </summary>
internal sealed class SpamHandlingExecutor() : Executor<DetectionResult>("SpamHandlingExecutor")
{
    public override async ValueTask HandleAsync(DetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // 🛡️ 防御性检查：确保只处理垃圾邮件
        if (message.spamDecision != SpamDecision.Spam)
            throw new InvalidOperationException(
                "SpamHandlingExecutor 只应处理 Spam 类型的邮件，请检查路由配置。"
            );

        // 记录垃圾邮件（实际项目中可写入数据库或日志系统）
        await context.YieldOutputAsync(
            $"🚫 垃圾邮件已拦截: {message.Reason}",
            cancellationToken
        );
    }
}