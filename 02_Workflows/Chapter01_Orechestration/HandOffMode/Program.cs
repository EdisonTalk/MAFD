using CommonShared;
using HandOffMode.FunctionAgents;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

// Load Configuration
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
#endif
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();

// Step1. Create one ChatClient
var chatClient = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId);

// Step2. Create 3 function agents
var triageAgent = FunctionAgentFactory.CreateTriageAgent(chatClient);
var historyTutor = FunctionAgentFactory.CreateHistoryTutorAgent(chatClient);
var mathTutor = FunctionAgentFactory.CreateMathTutorAgent(chatClient);

// Step3. Build handoff workflow with routing rules
var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(triageAgent)
    .WithHandoffs(triageAgent, [mathTutor, historyTutor]) // Triage can route to either specialist
    .WithHandoffs([mathTutor, historyTutor], triageAgent) // Math or History tutor can return to triage
    .Build();

// Step4. Process multi-turn conversations
// Round1: What is the derivative of x^2?
// Round2: Tell me about World War 2
// Round3: Can you help me with calculus integration?
List<ChatMessage> messages = new();
while (true)
{
    Console.Write("User: ");
    string userInput = Console.ReadLine()!;
    messages.Add(new(ChatRole.User, userInput));

    // Execute workflow and process events
    var response = await workflow.AsAgent().RunAsync(messages);
    Console.WriteLine($"Agent: {response}\n");

    // Add new messages to conversation history
    messages.AddRange(response.Messages);
}