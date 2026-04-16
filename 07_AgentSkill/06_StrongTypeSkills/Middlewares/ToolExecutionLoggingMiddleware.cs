using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentSkillDemo.Middlewares;

internal class ToolExecutionLoggingMiddleware
{
    /// <summary>
    /// 简化版函数调用中间件 - 记录 Tool 执行日志
    /// </summary>
    public static async ValueTask<object?> ExecuteAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"\n→ 🍴Tool: {context.Function.Name}");
        var result = await next(context, cancellationToken);
        Console.WriteLine($"← 🥣Result: {result}");

        return result;
    }
}
