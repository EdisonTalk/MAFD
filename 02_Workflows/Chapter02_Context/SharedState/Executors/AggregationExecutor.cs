using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using SharedState.Models;
using System.Text.Json;

namespace SharedState.Executors;

internal sealed class AggregationExecutor : Executor<FileStats>
{
    private readonly AIAgent _aggregationAgent;
    private readonly AgentThread _thread;
    private readonly List<FileStats> _buffer = [];

    public AggregationExecutor(IChatClient chatClient) : base("AggregationExecutor")
    {
        // 创建 Agent 和对话线程
        this._aggregationAgent = chatClient.CreateAIAgent(
            "你是一个专业的文档统计结果输出大师，你可以将收到的JSON格式统计结果（如总词数、总段落数 以及 统计时间等）进行友好的信息输出给用户。",
            "output_agent",
            "Output user friendly message based on input document stats result"); ;
        this._thread = this._aggregationAgent.GetNewThread();
    }

    public override async ValueTask HandleAsync(FileStats message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        this._buffer.Add(message);
        if (this._buffer.Count < 2)
        {
            return;
        }

        int totalWords = this._buffer.Sum(x => x.WordCount);
        int totalParagraphs = this._buffer.Sum(x => x.ParagraphCount);

        var output = new
        {
            总词数 = totalWords,
            总段落数 = totalParagraphs,
            统计时间 = DateTimeOffset.UtcNow
        };

        var response = await this._aggregationAgent.RunAsync(JsonSerializer.Serialize(output), this._thread, cancellationToken: cancellationToken);

        await context.YieldOutputAsync(response.Text, cancellationToken);
    }
}