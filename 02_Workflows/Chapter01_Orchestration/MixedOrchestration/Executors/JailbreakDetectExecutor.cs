using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using MixedOrchestration.Events;
using MixedOrchestration.Models;
using System.Text.Json;

namespace MixedOrchestration.Executors;

/// <summary>
/// Jailbreak 检测专家：调用Agent进行AI提示词攻击检测
/// </summary>
public sealed class JailbreakDetectExecutor : Executor<ChatMessage, DetectionResult>
{
    private readonly AIAgent _detectorAgent;
    private readonly AgentThread _thread;

    public JailbreakDetectExecutor(AIAgent agent) : base("JailbreakDetectExecutor")
    {
        // 创建 Agent 和对话线程
        this._detectorAgent = agent;
        this._thread = this._detectorAgent.GetNewThread();
    }

    public override async ValueTask<DetectionResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Invoke the Jailbreak Detection Agent
        var response = await this._detectorAgent.RunAsync(message, this._thread, cancellationToken: cancellationToken);
        var detectionResult = JsonSerializer.Deserialize<DetectionResult>(response.Text)
            ?? throw new InvalidOperationException("❌ 反序列化检测结果失败");
        var detectMessage = detectionResult.IsJailbreak ? "DETECTED" : "SAFE";
        Console.WriteLine($"\n[JailbreakDetectExecutor] 🤖 AI提示词攻击检测");
        Console.WriteLine($"检测结果：{detectMessage}");

        // Send custom event if jailbreak is detected
        if (detectionResult.IsJailbreak)
            await context.AddEventAsync(new JailbreakDetectedEvent(detectionResult), cancellationToken);

        return detectionResult;
    }
}