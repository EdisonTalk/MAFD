using CustomExecutor.Events;
using CustomExecutor.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace CustomExecutor.Executors;
/// <summary>
/// 文案生成 Executor - 根据任务或反馈生成标语
/// </summary>
public class SloganWriterExecutor : Executor
{
    private readonly AIAgent _agent;
    private readonly AgentThread _thread;

    /// <summary>
    /// 初始化文案生成 Executor
    /// </summary>
    /// <param name="id">Executor 唯一标识</param>
    /// <param name="chatClient">AI 聊天客户端</param>
    public SloganWriterExecutor(string id, IChatClient chatClient) : base(id)
    {
        // 配置 Agent 选项
        ChatClientAgentOptions agentOptions = new(
            instructions: "你是一名专业的文案撰写专家。你将根据产品特性创作简洁有力的宣传标语。"
        )
        {
            ChatOptions = new()
            {
                // 配置结构化输出：要求返回 SloganResult JSON 格式
                ResponseFormat = ChatResponseFormat.ForJsonSchema<SloganResult>()
            }
        };

        // 创建 Agent 和对话线程
        this._agent = new ChatClientAgent(chatClient, agentOptions);
        this._thread = this._agent.GetNewThread();
    }

    /// <summary>
    /// 配置消息路由：支持两种输入类型
    /// </summary>
    protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder
            .AddHandler<string, SloganResult>(this.HandleInitialTaskAsync)      // 处理初始任务
            .AddHandler<FeedbackResult, SloganResult>(this.HandleFeedbackAsync);  // 处理反馈

    /// <summary>
    /// 处理初始任务（首次生成）
    /// </summary>
    private async ValueTask<SloganResult> HandleInitialTaskAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"✍️ [文案生成] 接收到任务: {message}");

        // 调用 Agent 生成标语
        var result = await this._agent.RunAsync(message, this._thread, cancellationToken: cancellationToken);

        // 反序列化结构化输出
        var sloganResult = JsonSerializer.Deserialize<SloganResult>(result.Text)
            ?? throw new InvalidOperationException("❌ 反序列化标语结果失败");

        Console.WriteLine($"📝 [文案生成] 生成标语: {sloganResult.Slogan}");

        // 发布自定义事件（将在后续定义）
        await context.AddEventAsync(new SloganGeneratedEvent(sloganResult), cancellationToken);

        return sloganResult;
    }

    /// <summary>
    /// 处理审核反馈（改进优化）
    /// </summary>
    private async ValueTask<SloganResult> HandleFeedbackAsync(
        FeedbackResult feedback,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 构造反馈消息
        var feedbackMessage = $"""
            以下是对你之前标语的审核反馈：
            评论: {feedback.Comments}
            评分: {feedback.Rating} / 10
            改进建议: {feedback.Actions}

            请根据反馈改进你的标语，使其更加精准有力。
            """;

        Console.WriteLine($"🔄 [文案生成] 接收到反馈，评分: {feedback.Rating}/10");

        // 调用 Agent 改进标语（保持对话上下文）
        var result = await this._agent.RunAsync(feedbackMessage, this._thread, cancellationToken: cancellationToken);

        var sloganResult = JsonSerializer.Deserialize<SloganResult>(result.Text)
            ?? throw new InvalidOperationException("❌ 反序列化标语结果失败");

        Console.WriteLine($"📝 [文案生成] 改进后标语: {sloganResult.Slogan}");

        // 发布事件
        await context.AddEventAsync(new SloganGeneratedEvent(sloganResult), cancellationToken);

        return sloganResult;
    }
}