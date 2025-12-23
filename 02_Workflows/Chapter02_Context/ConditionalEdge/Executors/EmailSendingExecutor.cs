using ConditionalEdge.Models;
using Microsoft.Agents.AI.Workflows;

namespace ConditionalEdge.Executors;

internal sealed class EmailSendingExecutor() : Executor<EmailResponse>("EmailSendingExecutor")
{
    public override async ValueTask HandleAsync(EmailResponse message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await context.YieldOutputAsync($"📤 Email 已发送：{message.Response}", cancellationToken);
    }
}