using LoopFlow.Constants;
using LoopFlow.Events;
using LoopFlow.Models;
using LoopFlow.Models.ValueObjects;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace LoopFlow.Executors;

internal sealed class BaselineQualityCheckExecutor : Executor<ReplyDraft>
{
    private readonly int _politenessThreshold;
    private readonly int _accuracyThreshold;
    private readonly IChatClient _chatClient;

    public BaselineQualityCheckExecutor(int politenessThreshold, int accuracyThreshold, IChatClient chatClient)
        : base("BaselineQualityCheck")
    {
        _politenessThreshold = politenessThreshold;
        _accuracyThreshold = accuracyThreshold;
        _chatClient = chatClient;
    }

    public override async ValueTask HandleAsync(ReplyDraft draft, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var attempt = await context.ReadOrInitStateAsync("attempt", () => 1, cancellationToken);

        // 使用 AI 进行多维度质检评分（结构化输出，严格标准）
        var prompt = $"""
        你是一位严格的客服质检专家。请对以下客服回复进行多维度评分（0-100分）：

        回复内容：{draft.Content}

        评分维度和严格标准：
        1. 礼貌度（0-100）：必须包含称呼语（您、亲）、感谢语（感谢、谢谢）、结束语（祝、期待）等，语气亲和温暖
           - 缺少任何一项扣20分
           - 语气生硬、机械扣10-30分
        
        2. 准确性（0-100）：必须提供具体的解决方案、明确的处理时间或有效的后续步骤
           - 只说"会处理"但无具体方案扣30分
           - 无明确时间承诺扣20分
           - 信息过于笼统扣10-20分
        
        3. 合规性（通过/不通过）：不得包含敏感词、不当表述、推诿责任的话语
           - 发现任何敏感词或不当表述直接判定为"不通过"

        请对每个维度进行严格评分，并在issues字段中列出所有发现的问题。
        """;

        // ⭐ 使用结构化输出：GetResponseAsync<T> 自动生成 JSON Schema 并反序列化
        var response = await _chatClient.GetResponseAsync<QualityReportDto>(prompt, cancellationToken: cancellationToken);
        var reportDto = response.Result;

        // 转换为业务模型
        var issues = reportDto.Issues
            .Select(i => new QualityIssue(i.Type, i.Description, i.ScoreImpact))
            .ToList();
        var report = new QualityReport(draft.TicketId, reportDto.PolitenessScore, 
            reportDto.AccuracyScore, reportDto.CompliancePassed, issues);

        await context.AddEventAsync(
            new BaselineQualityScoreEvent(draft.TicketId, attempt, 
                reportDto.PolitenessScore, reportDto.AccuracyScore, 
                reportDto.CompliancePassed), cancellationToken);

        await context.QueueStateUpdateAsync("attempt", ++attempt, cancellationToken);

        if (reportDto.PolitenessScore >= _politenessThreshold 
            && reportDto.AccuracyScore >= _accuracyThreshold 
            && reportDto.CompliancePassed)
        {
            await context.YieldOutputAsync(new TicketOutcome(draft.TicketId, "Approved", attempt, report), cancellationToken);
        }
        else
        {
            await context.SendMessageAsync(QualityCheckSignal.Revise, targetId: "BaselineReplyDraft", cancellationToken);
        }
    }
}