using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace ParallelExecution.Executors;

internal sealed class PricingStrategyExecutor : Executor<ChatMessage>
{
    private readonly List<ChatMessage> _messages = [];
    private readonly int _targetCount;

    public PricingStrategyExecutor(int targetCount) : base(nameof(PricingStrategyExecutor))
    {
        _targetCount = targetCount;
    }

    public override async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        this._messages.Add(message);
        Console.WriteLine($"📊 已收集 {_messages.Count}/{_targetCount} 个平台数据 - 来自 {message.AuthorName}");

        if (this._messages.Count == this._targetCount)
        {
            var platformData = string.Join("\n", this._messages.Select(m => $"• {m.AuthorName}: {m.Text}"));
            var strategyReport = $@"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📊 多平台价格汇总（共 {this._messages.Count} 个平台）
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

{platformData}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
💡 智能定价建议
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
基于以上数据，建议分析竞争对手价格区间，制定差异化定价策略。
考虑因素：库存压力、配送成本、平台佣金率、目标利润率。";

            await context.YieldOutputAsync(strategyReport, cancellationToken);

            Console.WriteLine("✨ Fan-in 定价策略生成完成");
        }
    }
}