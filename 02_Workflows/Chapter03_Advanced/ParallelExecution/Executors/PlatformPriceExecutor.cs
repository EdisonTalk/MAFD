using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace ParallelExecution.Executors;

internal sealed class PlatformPriceExecutor : Executor<ChatMessage>
{
    private readonly string _instructions;
    private readonly IChatClient _chatClient;

    public PlatformPriceExecutor(string id, IChatClient chatClient, string platformInstructions)
        : base(id)
    {
        _chatClient = chatClient;
        _instructions = platformInstructions;
    }

    public override async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, _instructions),
            message
        };

        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        var replyMessage = new ChatMessage(ChatRole.Assistant, response.Text ?? string.Empty)
        {
            AuthorName = this.Id
        };

        await context.SendMessageAsync(replyMessage, cancellationToken: cancellationToken);
        Console.WriteLine($"✅ {this.Id} 完成查询");
    }
}