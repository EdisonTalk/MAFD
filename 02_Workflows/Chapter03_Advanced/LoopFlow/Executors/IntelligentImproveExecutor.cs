using LoopFlow.Constants;
using LoopFlow.Events;
using LoopFlow.Models.ValueObjects;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace LoopFlow.Executors;

internal sealed class IntelligentImproveExecutor : Executor<QualityReport>
{
    private readonly TicketRequest _ticket;
    private readonly IChatClient _chatClient;

    public IntelligentImproveExecutor(TicketRequest ticket, IChatClient chatClient) : base("IntelligentImprove")
    {
        _ticket = ticket;
        _chatClient = chatClient;
    }

    public override async ValueTask HandleAsync(QualityReport report, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        int attempt = await context.ReadOrInitStateAsync("attempt", () => 1, cancellationToken);

        // 构建改进提示词，基于质检反馈
        var issuesSummary = string.Join("\n", report.Issues.Select(i => $"- {i.Type}: {i.Description}"));

        var prompt = $"""
        你是一位客服优化专家。请根据以下质检反馈，改进客服回复内容：

        原始问题：{_ticket.Query}
        产品类别：{_ticket.Category}
        优先级：{_ticket.Priority}

        当前评分：
        - 礼貌度：{report.PolitenessScore}/100 (要求≥{QualityCheckConstants.PolitenessThreshold})
        - 准确性：{report.AccuracyScore}/100 (要求≥{QualityCheckConstants.AccuracyThreshold})
        - 合规性：{(report.CompliancePassed ? "通过" : "不通过")}

        发现的问题：
        {issuesSummary}

        请生成一条改进后的客服回复，针对性解决上述问题：
        1. 如果礼貌度不足，增加称呼语、感谢语，使用更亲和的表述
        2. 如果准确性不足，补充具体的解决方案、处理时间、后续步骤
        3. 如果合规性不通过，移除敏感词，规范表述
        4. 字数控制在80-100字

        直接返回改进后的回复内容，不要添加任何前缀或说明。
        """;

        var response = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
        var improvedContent = response.Text ?? "抱歉，我们会尽快处理您的问题。";

        await context.AddEventAsync(new LoopProgressEvent(_ticket.Id, attempt, 
            report.PolitenessScore, report.AccuracyScore, 
            report.CompliancePassed, "Improve"), cancellationToken);
        Console.WriteLine($"🔧 第 {attempt} 次智能改进完成");
        Console.WriteLine($"📝 改进后内容：{improvedContent}");

        // 触发下一次生成（使用改进后的内容作为上下文）
        await context.SendMessageAsync(QualityCheckSignal.Revise, targetId: "AdaptiveReplyDraft", cancellationToken);
    }
}