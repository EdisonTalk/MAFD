using SwitchCaseV1.Constants;
using SwitchCaseV1.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace SwitchCaseV1.Executors;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 💼 Email Assistant Executor
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
/// <summary>
/// 邮件助手执行器（仅处理正常邮件）
/// 输入: DetectionResult（必须是 NotSpam）
/// 输出: EmailResponse（AI 生成的回复）
/// </summary>
internal sealed class EmailAssistantExecutor : Executor<DetectionResult, EmailResponse>
{
    private readonly AIAgent _agent;
    private readonly AgentThread _thread;

    public EmailAssistantExecutor(AIAgent agent) : base("EmailAssistantExecutor")
    {
        // 创建 Agent 和对话线程
        this._agent = agent;
        this._thread = this._agent.GetNewThread();
    }

    public override async ValueTask<EmailResponse> HandleAsync(DetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // 🛡️ 防御性检查：确保只处理正常邮件
        if (message.spamDecision == SpamDecision.Spam)
            throw new InvalidOperationException(
                "EmailAssistantExecutor 不应处理垃圾邮件，请检查路由配置。"
            );

        // 1️⃣ 从 Shared State 读取原始邮件内容
        var email = await context.ReadStateAsync<EmailMessage>(
            message.EmailId,
            scopeName: EmailStateConstants.EmailStateScope,
            cancellationToken
        ) ?? throw new InvalidOperationException($"找不到 EmailId={message.EmailId} 的邮件内容");

        // 2️⃣ 调用 AI Agent 生成回复
        var agentResponse = await _agent.RunAsync(
            email.EmailContent,
            _thread,
            cancellationToken: cancellationToken
        );

        // 3️⃣ 解析结构化输出
        var emailResponse = JsonSerializer.Deserialize<EmailResponse>(agentResponse.Text)
            ?? throw new InvalidOperationException("无法解析 Email Assistant 响应");

        return emailResponse;
    }
}