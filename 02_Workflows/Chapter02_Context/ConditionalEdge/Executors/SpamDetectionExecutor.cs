using ConditionalEdge.Constants;
using ConditionalEdge.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace ConditionalEdge.Executors;

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
        var trackedEmail = new EmailMessage
        {
            EmailId = Guid.NewGuid().ToString("N"),
            EmailContent = message.Text
        };

        await context.QueueStateUpdateAsync(trackedEmail.EmailId, trackedEmail, scopeName: EmailStateConstants.EmailStateScope, cancellationToken);

        var agentResponse = await _agent.RunAsync(message, cancellationToken: cancellationToken);
        var detectionResult = JsonSerializer.Deserialize<DetectionResult>(agentResponse.Text)
            ?? throw new InvalidOperationException("无法解析 Spam Detection 响应。");

        detectionResult.EmailId = trackedEmail.EmailId;
        return detectionResult;
    }
}