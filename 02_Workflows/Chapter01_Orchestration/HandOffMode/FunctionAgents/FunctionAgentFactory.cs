using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

namespace HandOffMode.FunctionAgents;

public class FunctionAgentFactory
{
    public static ChatClientAgent CreateTriageAgent(ChatClient client)
    {
        return client.CreateAIAgent(
            "You determine which agent to use based on the user's homework question. ALWAYS handoff to another agent.",
            "triage_agent",
            "Routes messages to the appropriate specialist agent");
    }

    public static ChatClientAgent CreateHistoryTutorAgent(ChatClient client)
    {
        return client.CreateAIAgent(
            "You provide assistance with historical queries. Explain important events and context clearly. Please only respond about history.",
            "history_tutor",
            "Specialist agent for historical questions");
    }

    public static ChatClientAgent CreateMathTutorAgent(ChatClient client)
    {
        return client.CreateAIAgent(
            "You provide help with math problems. Explain your reasoning at each step and include examples. Please only respond about math.",
            "math_tutor",
            "Specialist agent for mathematical questions");
    }
}