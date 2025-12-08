using MixedOrchestration.Models;
using Microsoft.Agents.AI.Workflows;

namespace MixedOrchestration.Events;

/// <summary>
/// 自定义事件:审核反馈完成
/// </summary>
public sealed class FeedbackFinishedEvent : WorkflowEvent
{
    private readonly FeedbackResult _feedbackResult;

    public FeedbackFinishedEvent(FeedbackResult feedbackResult) 
        : base(feedbackResult)
    {
        this._feedbackResult = feedbackResult;
    }

    public override string ToString() =>
        $"""
        📊 [审核反馈]
        评分: {_feedbackResult.Rating}/10
        评论: {_feedbackResult.Comments}
        建议: {_feedbackResult.Actions}
        """;
}
