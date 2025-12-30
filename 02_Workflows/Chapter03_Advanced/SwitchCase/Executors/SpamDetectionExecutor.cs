using SwitchCaseV1.Constants;
using SwitchCaseV1.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace SwitchCaseV1.Executors;

internal sealed class SpamDetectionExecutor : Executor<ChatMessage, DetectionResult>
{
    private readonly AIAgent _agent;
    private readonly AgentThread _thread;

    public SpamDetectionExecutor(AIAgent agent) : base("SpamDetectionExecutor")
    {
        // 创建 Agent 和对话线程
        this._agent = agent;
        this._thread = this._agent.GetNewThread();
    }

    public override async ValueTask<DetectionResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // 1️⃣ 生成唯一邮件ID并保存内容到 Shared State
        var trackedEmail = new EmailMessage
        {
            EmailId = Guid.NewGuid().ToString("N"),
            EmailContent = message.Text
        };

        await context.QueueStateUpdateAsync(
            trackedEmail.EmailId,
            trackedEmail,
            scopeName: EmailStateConstants.EmailStateScope,
            cancellationToken
        );

        // 2️⃣ 调用 AI Agent 进行三分类检测
        var agentResponse = await _agent.RunAsync(
            message,
            _thread,
            cancellationToken: cancellationToken
        );

        // 3️⃣ 解析结构化输出
        var detection = JsonSerializer.Deserialize<DetectionResult>(agentResponse.Text)
            ?? throw new InvalidOperationException("无法解析 Spam Detection 响应");

        // 4️⃣ 关联 EmailId（供下游 Executor 查找原始内容）
        detection.EmailId = trackedEmail.EmailId;

        return detection;
    }
}