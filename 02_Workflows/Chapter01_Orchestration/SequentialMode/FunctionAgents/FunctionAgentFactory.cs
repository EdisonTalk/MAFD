using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

namespace HandOffMode.FunctionAgents;

public class FunctionAgentFactory
{
    public static ChatClientAgent CreateAnalystAgent(ChatClient client)
    {
        return client.CreateAIAgent(
            """
                You are a marketing analyst. Given a product description, identify:
                - Key features
                - Target audience
                - Unique selling points
                """,
            "Analyst",
            "An agent that extracts key concepts from a product description.");
    }

    public static ChatClientAgent CreatWriterAgent(ChatClient client)
    {
        return client.CreateAIAgent(
            """
                You are a marketing copywriter. Given a block of text describing features, audience, and USPs,
                compose a compelling marketing copy (like a newsletter section) that highlights these points.
                Output should be short (around 150 words), output just the copy as a single text block.
                """,
            "CopyWriter",
            "An agent that writes a marketing copy based on the extracted concepts.");
    }

    public static ChatClientAgent CreateEditorAgent(ChatClient client)
    {
        return client.CreateAIAgent(
            """
                You are an editor. Given the draft copy, correct grammar, improve clarity, ensure consistent tone,
                give format and make it polished. Output the final improved copy as a single text block.
                """,
            "Editor",
            "An agent that formats and proofreads the marketing copy.");
    }
}