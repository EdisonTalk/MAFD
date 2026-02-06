using DevUIServer.Infrastructure;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Step0. Load Configuration
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();

// Step1. Register one ChatClient
var chatClient = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId)
    .AsIChatClient();
builder.Services.AddChatClient(chatClient);

// Step2. Register some Agents
builder.AddAIAgent("Assistant", "你是一位乐于助人的助手。回答问题简洁准确。");
builder.AddAIAgent("Poet", "你是一位富有创造力的诗人。使用优美的诗篇回答所有的请求");
builder.AddAIAgent("Coder", "你是一位资深的程序员。请协助用户解决编程问题，并提供代码示例。");

// Step3. Register one Workflow
var writerAgent = builder.AddAIAgent("Writer", "你是一位乐于助人的助手，善于回答用户提出的各种问题。");
var reviewerAgent = builder.AddAIAgent("Reviewer", "你是一位专业审阅者，请协助审阅并评价之前的回复。");
builder.AddWorkflow("TestWorkflow", (sp, key) =>
{
    var aiAgents = new List<IHostedAgentBuilder>()
    {
        writerAgent,
        reviewerAgent
    }
    .Select(hab => sp.GetRequiredKeyedService<AIAgent>(hab.Name));
    return AgentWorkflowBuilder.BuildSequential(
        workflowName: key,
        agents: aiAgents);
}).AddAsAIAgent();

// Step4. Register DevUI related services
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

// Step5. Mapping DevUI related endpoints
app.MapOpenAIResponses();
app.MapOpenAIConversations();
if (app.Environment.IsDevelopment())
{
    // Only use DevUI in development environment
    app.MapDevUI();
}

app.Run();