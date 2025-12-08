using CustomExecutor.Events;
using CustomExecutor.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace CustomExecutor.Executors;
/// <summary>
/// 审核反馈 Executor - 评估标语质量并提供反馈
/// </summary>
public sealed class FeedbackExecutor : Executor<SloganResult>
{
    private readonly AIAgent _agent;
    private readonly AgentThread _thread;
    private int _attempts = 0;

    /// <summary>
    /// 最低评分要求（1-10分）
    /// </summary>
    public int MinimumRating { get; init; } = 8;

    /// <summary>
    /// 最大尝试次数
    /// </summary>
    public int MaxAttempts { get; init; } = 3;

    /// <summary>
    /// 初始化审核反馈 Executor
    /// </summary>
    /// <param name="id">Executor 唯一标识</param>
    /// <param name="chatClient">AI 聊天客户端</param>
    public FeedbackExecutor(string id, IChatClient chatClient) 
        : base(id)
    {
        // 配置 Agent 选项
        ChatClientAgentOptions agentOptions = new(
            instructions: "你是一名专业的文案审核专家。你将评估标语的质量，并提供改进建议。"
        )
        {
            ChatOptions = new()
            {
                // 配置结构化输出：要求返回 FeedbackResult JSON 格式
                ResponseFormat = ChatResponseFormat.ForJsonSchema<FeedbackResult>()
            }
        };

        this._agent = new ChatClientAgent(chatClient, agentOptions);
        this._thread = this._agent.GetNewThread();
    }

    /// <summary>
    /// 处理标语审核
    /// </summary>
    public override async ValueTask HandleAsync(
        SloganResult slogan,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 构造审核消息
        var reviewMessage = $"""
            请审核以下标语：
            任务: {slogan.Task}
            标语: {slogan.Slogan}
            
            请提供：
            1. 详细的评论（comments）
            2. 质量评分（rating，1-10分）
            3. 改进建议（actions）
            """;

        Console.WriteLine($"🔍 [质量审核] 开始审核标语: {slogan.Slogan}");

        // 调用 Agent 进行审核
        var response = await this._agent.RunAsync(reviewMessage, this._thread, cancellationToken: cancellationToken);

        // 反序列化反馈结果
        var feedback = JsonSerializer.Deserialize<FeedbackResult>(response.Text)
            ?? throw new InvalidOperationException("❌ 反序列化反馈结果失败");

        Console.WriteLine($"📊 [质量审核] 评分: {feedback.Rating}/10");

        // 发布自定义事件（将在后续定义）
        await context.AddEventAsync(new FeedbackFinishedEvent(feedback), cancellationToken);

        // 业务逻辑：判断是否通过审核
        if (feedback.Rating >= this.MinimumRating)
        {
            // ✅ 通过审核
            await context.YieldOutputAsync(
                $"""
                ✅ 标语已通过审核！
                
                任务: {slogan.Task}
                标语: {slogan.Slogan}
                评分: {feedback.Rating}/10
                评论: {feedback.Comments}
                """,
                cancellationToken
            );
            Console.WriteLine($"✅ [质量审核] 标语通过审核");
            return;
        }

        // ❌ 未通过审核，检查尝试次数
        if (this._attempts >= this.MaxAttempts)
        {
            // 达到最大尝试次数，输出最终版本
            await context.YieldOutputAsync(
                $"""
                ⚠️ 标语在 {this.MaxAttempts} 次尝试后未达到最低评分要求。
                
                最终标语: {slogan.Slogan}
                最终评分: {feedback.Rating}/10
                评论: {feedback.Comments}
                """,
                cancellationToken
            );
            Console.WriteLine($"⚠️ [质量审核] 达到最大尝试次数，终止流程");
            return;
        }

        // 🔄 继续循环：发送反馈消息回到 SloganWriterExecutor
        await context.SendMessageAsync(feedback, cancellationToken: cancellationToken);
        this._attempts++;
        Console.WriteLine($"🔄 [质量审核] 发送反馈，第 {this._attempts} 次尝试");
    }
}
