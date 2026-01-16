using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
namespace SequentialFlow.Factories;

public static class CustomAgentFactory
{
    public static ChatClientAgent CreateAgent(string name, string role, IChatClient client)
    {
        return new ChatClientAgent(
            chatClient: client,
            instructions: $"You are a {role}. Your goal is to complete the task based on the input provided. Please output the result directly.",
            name: name
        );
    }
}