using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using MixedOrchestration.Models;
using System.Text.Json;

namespace MixedOrchestration.Executors;

/// <summary>
/// 最终输出执行器：展示工作流的最终结果
/// </summary>
public sealed class FinalOutputExecutor : Executor<DetectionResult, UserRequestResult>
{
    private readonly AIAgent _responseOutputAgent;
    private readonly AgentThread _thread;

    public FinalOutputExecutor(AIAgent agent) : base("FinalOutput")
    {
        // 创建 Agent 和对话线程
        this._responseOutputAgent = agent;
        this._thread = this._responseOutputAgent.GetNewThread();
    }

    public override async ValueTask<UserRequestResult> HandleAsync(DetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // 调用大模型获取最终回复
        var response = await this._responseOutputAgent.RunAsync(message.UserInput, this._thread, cancellationToken: cancellationToken);
        var requestResult = JsonSerializer.Deserialize<UserRequestResult>(response.Text)
            ?? throw new InvalidOperationException("❌ 反序列化处理结果失败");

        //await context.YieldOutputAsync($"📤 最终回复: {requestResult.FinalResponse}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[{Id}] 📤 最终回复：");
        Console.WriteLine(requestResult.FinalResponse);
        Console.WriteLine($"\n✅ 工作流执行完成\n");
        Console.ResetColor();

        return requestResult;
    }
}