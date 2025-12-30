using LoopFlow.Constants;
using LoopFlow.Models.ValueObjects;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace LoopFlow.Executors;

internal sealed class AdaptiveReplyDraftExecutor : Executor<QualityCheckSignal>
{
    private readonly TicketRequest _ticket;
    private readonly IChatClient _chatClient;

    public AdaptiveReplyDraftExecutor(TicketRequest ticket, IChatClient chatClient) : base("AdaptiveReplyDraft")
    {
        _ticket = ticket;
        _chatClient = chatClient;
    }

    public override async ValueTask HandleAsync(QualityCheckSignal message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        int attempt = await context.ReadOrInitStateAsync("attempt", () => 0, cancellationToken);
        attempt++;
        await context.QueueStateUpdateAsync("attempt", attempt, cancellationToken);

        // 使用 AI 生成客服回复（渐进式生成策略）
        var prompt = attempt == 1
            ? $"""
            你是一位电商客服。请针对以下客户问题生成一条简短回复（刻意保持简短、缺少礼貌用语）：

            客户问题：{_ticket.Query}
            产品类别：{_ticket.Category}

            要求：
            1. 只用1-2句话回答，不要称呼语和感谢语
            2. 只说结论，不提供具体处理时间
            3. 字数控制在30字以内

            直接返回回复内容，不要添加任何前缀或说明。
            """
            : $"""
            你是一位专业的电商客服。请针对以下客户问题生成一条改进后的回复：

            客户问题：{_ticket.Query}
            产品类别：{_ticket.Category}
            优先级：{_ticket.Priority}

            要求：
            1. 语气亲和、专业，使用恰当的称呼和感谢语
            2. 提供具体的解决方案或处理时间
            3. 符合客服规范，不包含敏感词
            4. 字数控制在80-100字

            直接返回回复内容，不要添加任何前缀或说明。
            """;

        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
        var content = response.Text ?? "抱歉，我们会尽快处理您的问题。";

        Console.WriteLine($"✍️ 第 {attempt} 次生成回复草稿 (策略: {(attempt == 1 ? "简化版" : "完整版")})");
        Console.WriteLine($"📝 回复内容：{content}");

        var draft = new ReplyDraft(_ticket.Id, content, attempt);
        await context.SendMessageAsync(draft, targetId: "AdaptiveQualityCheck", cancellationToken);
    }
}