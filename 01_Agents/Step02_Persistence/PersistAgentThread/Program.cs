// Load configuration
using CommonShared;
using Microsoft.Extensions.Configuration;
using OpenAI;
using PersistAgentThread.Infrastructure;
using System.ClientModel;
using System.Text.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
#endif
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();

// Step1. Create one Agent
var mazdaAgent = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId)
    .CreateAIAgent(name: "Powerful Assistant", instructions: "You are a helpful assistant who responds user message in Mazda cars.");
// Step2. Start a new thread
var userMessage = "Hello, can you tell me about the Mazda 3?";
Console.WriteLine($"User> {userMessage}");
var thread = mazdaAgent.GetNewThread();
var agentResponse = await mazdaAgent.RunAsync(userMessage, thread);
Console.WriteLine($"Agent> {agentResponse}");
// Step3. Persist the thread
var serializedThread = thread.Serialize(JsonSerializerOptions.Web).GetRawText();
var conversation = new AgentConversation(serializedThread);
var dbContext = new AgentConversationDbContext();
dbContext.Database.EnsureCreated();
dbContext.AgentConversations.Add(conversation);
await dbContext.SaveChangesAsync();
// Step4. Load the thread later from database
var savedConversation = dbContext.AgentConversations.First(c => c.Id == conversation.Id);
var loadedThread = JsonSerializer.Deserialize<JsonElement>(savedConversation.Context, JsonSerializerOptions.Web);
var resumedThread = mazdaAgent.DeserializeThread(loadedThread, JsonSerializerOptions.Web);
// Step5. Continue the conversation
Console.WriteLine();
userMessage = "What are the features of this car?";
Console.WriteLine($"User> {userMessage}");
agentResponse = await mazdaAgent.RunAsync(userMessage, resumedThread);
Console.WriteLine($"Agent> {agentResponse}");

Console.ReadKey();