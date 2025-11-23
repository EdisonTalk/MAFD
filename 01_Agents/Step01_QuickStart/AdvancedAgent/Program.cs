using AdvancedAgent.Plugins;
using CommonShared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.Connectors.InMemory;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OpenAI;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.ClientModel;
using System.Text.Json;

// Load configuration
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
#endif
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();

#region 01-Using an Agent as a function tool
//// Step1. Create an AI agent that uses a weather service plugin
//var weatherAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(
//        instructions: "You answer questions about the weather.",
//        name: "WeatherAgent",
//        description: "An agent that answers questions about the weather.",
//        tools: [AIFunctionFactory.Create(WeatherServicePlugin.GetWeatherAsync)]);
//// Step2. Create another AI agent that uses the weather agent as a function tool
//var mainAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are a helpful assistant who responds message in Chinese.", tools: [weatherAgent.AsAIFunction()]);
//// Step3. Test the portal agent
//Console.WriteLine(await mainAgent.RunAsync("What is the weather like in Chengdu?"));
#endregion

#region 02-Exposing an Agent as a MCP tool
//var jokerAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");
//// Expose the agent as a MCP tool
//var jokerMcpTool = McpServerTool.Create(jokerAgent.AsAIFunction());
//// Create a MCP server and register the tool
//// Register the MCP server with StdIO transport and expose the tool via the server.
//var builder = Host.CreateEmptyApplicationBuilder(settings: null);
//builder.Services
//    .AddMcpServer()
//    .WithStdioServerTransport()
//    .WithTools([jokerMcpTool]);
//await builder
//    .Build()
//    .RunAsync();
#endregion

#region 03-Persisted Conversations
//// Step1. Create an AI agent
//var jokerAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");
//// Step2. Start a new thread for the agent conversation
//var thread = jokerAgent.GetNewThread();
//// Step3. Run the agent with a new thread
//Console.WriteLine(await jokerAgent.RunAsync("Tell me a joke about a pirate.", thread));
//Console.WriteLine("==> Now user leaves the chat, system save the conversation to local storage.");
//// Step4. Serialize the thread state to a JsonElement, so that it can be persisted for later use
//var serializedThread = thread.Serialize();
//// Step5. Save the serialized thread to a file (for demonstration purposes)
//var tempFilePath = Path.GetTempFileName();
//await File.WriteAllTextAsync(tempFilePath, JsonSerializer.Serialize(serializedThread));
//// Step6. Deserialize the thread state after loading from storage.
//Console.WriteLine("==> Now user join the chat again, system starting to load last conversation.");
//var reoladedSerializedThread = JsonSerializer.Deserialize<JsonElement>(await File.ReadAllTextAsync(tempFilePath));
//var resumedThread = jokerAgent.DeserializeThread(reoladedSerializedThread);
//// Step7. Run the agent with the resumed thread
//Console.WriteLine(await jokerAgent.RunAsync("Now tell the same joke in the voice of a pirate, and add some emojis to the joke.", resumedThread));
#endregion

#region 04-The 3rd party thread storage
//// Create a shared in-memory vector store to store the chat messages.
//var vectorStore = new InMemoryVectorStore();
//// Create an AI agent that uses the vector store to persist its conversations.
//var jokerAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(new ChatClientAgentOptions
//    {
//        Name = "Joker",
//        Instructions = "You are good at telling jokes.",
//        ChatMessageStoreFactory = ctx =>
//        {
//            // Create a new chat message store for this agent that stores the messages in a vector store.
//            // Each thread must get its own copy of the VectorChatMessageStore, since the store
//            // also contains the id that the thread is stored under.
//            return new VectorChatMessageStore(vectorStore, ctx.SerializedState, ctx.JsonSerializerOptions);
//        }
//    });
//// Start a new thread for the agent conversation.
//var thread = jokerAgent.GetNewThread();
//// Run the agent with a new thread.
//var userMessage = "Tell me a joke about a pirate.";
//Console.WriteLine($"User> {userMessage}");
//Console.WriteLine($"Agent> " + await jokerAgent.RunAsync(userMessage, thread));
//// Assume user leaves the chat, system saves the conversation to vector storage.
//Console.WriteLine("\n[DEBUG] Now user leaves the chat, system save the conversation to vector storage.");
//var serializedThread = thread.Serialize();
//Console.WriteLine("[DEBUG] Serialized thread ---\n");
//Console.WriteLine(JsonSerializer.Serialize(serializedThread, new JsonSerializerOptions { WriteIndented = true }));
//// Assume user joins the chat again, system starts to load last conversation.
//Console.WriteLine("\n[DEBUG] Now user join the chat again, system starting to load last conversation.\n");
//var resumedThread = jokerAgent.DeserializeThread(serializedThread);
//// Run the agent with the resumed thread.
//userMessage = "Now tell the same joke in the voice of a pirate, and add some emojis to the joke.";
//Console.WriteLine($"User> {userMessage}");
//Console.WriteLine($"Agent> " + await jokerAgent.RunAsync(userMessage, resumedThread));
//// Check the thread is stored in the vector store.
//var messageStore = resumedThread.GetService<VectorChatMessageStore>()!;
//Console.WriteLine($"\n[DEBUG] Thread is stored in vector store under key: {messageStore.ThreadDbKey}");
#endregion

#region 05-Adding middleware to Agent
//// Step1. Create an AI agent that uses a weather service plugin
//var baseAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(
//        instructions: "You are an AI assistant that helps people find information.",
//        tools: [AIFunctionFactory.Create(DateTimePlugin.GetDateTime)]);
//// Step2. Create a middleware that logs the input and output messages
//async ValueTask<object?> CustomFunctionCallingMiddleware(
//    AIAgent agent,
//    FunctionInvocationContext context,
//    Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
//    CancellationToken cancellationToken)
//{
//    Console.WriteLine($"[LOG] Function Name: {context!.Function.Name}");
//    var result = await next(context, cancellationToken);
//    Console.WriteLine($"[LOG] Function Call Result: {result}");

//    return result;
//}
//// Step3. Add the middleware to the agent
//var middlewareEnabledAgent = baseAgent
//    .AsBuilder()
//        .Use(CustomFunctionCallingMiddleware)
//    .Build();
//// Step4. Test the agent with middleware
//var userMessage = "Hi, what's the current time?";
//Console.WriteLine($"User> {userMessage}");
//var agentResponse = await middlewareEnabledAgent.RunAsync(userMessage);
//Console.WriteLine($"Agent> {agentResponse}");
#endregion

#region 06-Enable observability for Agents
// Create a TracerProvider that exports to the console
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("agent-telemetry-source")
    .AddConsoleExporter()
    .Build();
// Create the agent and enable OpenTelemetry instrumentation
var agent = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId)
    .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker")
    .AsBuilder()
    .UseOpenTelemetry(sourceName: "agent-telemetry-source")
    .Build();
// Run the agent and generate telemetry
var userMessage = "Tell me a joke about a pirate.";
Console.WriteLine($"User> {userMessage}");
Console.WriteLine($"Agent> {await agent.RunAsync(userMessage)}");
#endregion

Console.ReadKey();