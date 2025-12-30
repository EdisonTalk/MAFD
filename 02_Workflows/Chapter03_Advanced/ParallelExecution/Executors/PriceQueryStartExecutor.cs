using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ParallelExecution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelExecution.Executors;

internal sealed class PriceQueryStartExecutor() : Executor<PriceQueryDto>(nameof(PriceQueryStartExecutor))
{
    public override async ValueTask HandleAsync(PriceQueryDto query, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var userPrompt = $@"商品ID: {query.ProductId}
商品名称: {query.ProductName}
目标区域: {query.TargetRegion}

请查询该商品在你的平台上的当前价格、库存状态和配送信息。";
        await context.SendMessageAsync(new ChatMessage(ChatRole.User, userPrompt), cancellationToken: cancellationToken);
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken: cancellationToken);
        
        Console.WriteLine("📡 Fan-out 价格查询广播已发送");
    }
}