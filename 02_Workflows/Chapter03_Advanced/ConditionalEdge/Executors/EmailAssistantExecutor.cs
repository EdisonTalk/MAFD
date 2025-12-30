using ConditionalEdge.Constants;
using ConditionalEdge.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace ConditionalEdge.Executors;

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
        if (message.IsSpam)
            throw new InvalidOperationException("Spam 邮件不应进入 EmailAssistantExecutor。");

        var email = await context.ReadStateAsync<EmailMessage>(message.EmailId, scopeName: EmailStateConstants.EmailStateScope, cancellationToken)
            ?? throw new InvalidOperationException("找不到对应 Email 内容。");
        var agentResponse = await _agent.RunAsync(email.EmailContent, _thread, cancellationToken: cancellationToken);
        var emailResponse =  JsonSerializer.Deserialize<EmailResponse>(agentResponse.Text)
            ?? throw new InvalidOperationException("无法解析 Email Assistant 响应。");

        return emailResponse;
    }
}