using LoopFlow.Constants;
using LoopFlow.Models.ValueObjects;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace LoopFlow.Executors;

internal sealed class BaselineReplyDraftExecutor : Executor<QualityCheckSignal>
{
    private readonly TicketRequest _ticket;
    private readonly IChatClient _chatClient;

    public BaselineReplyDraftExecutor(TicketRequest ticket, IChatClient chatClient) : base("BaselineReplyDraft")
    {
        _ticket = ticket;
        _chatClient = chatClient;
    }

    public override async ValueTask HandleAsync(QualityCheckSignal message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        int attempt = await context.ReadOrInitStateAsync("attempt", () => 0, cancellationToken);
        attempt++;
        await context.QueueStateUpdateAsync("attempt", attempt, cancellationToken);

        var prompt = $"""
            你是一位电商客服。请针对以下客户问题生成一条简短回复：

            客户问题：{_ticket.Query}
            产品类别：{_ticket.Category}

            直接返回回复内容，不要添加任何前缀或说明。
            """;
        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
        Console.WriteLine($"✍️ 第 {attempt} 次生成回复草稿完成");
        var content = response.Text ?? "抱歉，我们会尽快处理您的问题。";
        Console.WriteLine($"📝 回复内容：{content}");

        var draft = new ReplyDraft(_ticket.Id, content, attempt);
        await context.SendMessageAsync(draft, targetId: "BaselineQualityCheck", cancellationToken);
    }
}