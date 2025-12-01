using CommonShared;
using HandOffMode.FunctionAgents;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.Text;

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
var analyst = FunctionAgentFactory.CreateAnalystAgent(chatClient);
var writer = FunctionAgentFactory.CreatWriterAgent(chatClient);
var editor = FunctionAgentFactory.CreateEditorAgent(chatClient);

// Step3. Build a sequential workflow
var workflow = AgentWorkflowBuilder.BuildSequential(
    "content-team-workflow", 
    [analyst, writer, editor]);

// Step4. Process conversation
var userMessage = "Please help to introduce our new product: An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hours.";
Console.Write($"User: {userMessage}\n");
// Execute the workflow via RunAsync
//var response = await workflow.AsAgent().RunAsync(userMessage);
//Console.WriteLine($"Agent: {response}");
// Execute the workflow via StreamAsync with event tracking
await using (StreamingRun run = await InProcessExecution.StreamAsync(workflow, userMessage))
{
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true)); // Enable event emitting

    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("Process Tracking");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine();

    var result = new List<ChatMessage>();
    var stageOutput = new StringBuilder();
    int stepNumber = 1;
    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        if (evt is AgentRunUpdateEvent updatedEvent)
        {
            stageOutput.Append($"{updatedEvent.Data} ");
        }
        else if (evt is ExecutorCompletedEvent completedEvent)
        {
            if (stageOutput.Length > 0)
            {
                Console.WriteLine($"Step {stepNumber}: {completedEvent.ExecutorId}");
                Console.WriteLine($"Output: {stageOutput.ToString()}\n");
                stepNumber++;
                stageOutput.Clear();
            }
        }
        else if (evt is WorkflowOutputEvent endEvent)
        {
            result = (List<ChatMessage>)endEvent.Data!;
            break;
        }
    }

    // Display final result
    foreach (var message in result.Skip(1))
        Console.WriteLine($"Agent: {message.Text}");
}
Console.WriteLine("\nTask Finished!");
Console.ReadKey();