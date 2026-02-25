using AgUIServer.Infrastructure;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

// Step0. Create WebApplication builder
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();

// Step1. Register AG-UI services
builder.Services.AddAGUI();

var app = builder.Build();

// Step2. Load Configuration
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();

// Step3. Create one ChatClient
var chatClient = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId)
    .AsIChatClient();

// Step4. Create one AI Agent
var agent = chatClient.AsAIAgent(
    name: "AGUI-Assistant",
    instructions: "你是一个友好的AI助手，请使用中文回答用户的问题。");
Console.WriteLine("✅ AI Agent 创建成功");

// Step5. Mapping AG-UI Endpoints
app.MapAGUI("/", agent);

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🚀 AG-UI Server 已启动");
Console.WriteLine("📍 端点地址: https://localhost:8443/");
Console.WriteLine("💡 使用 Ctrl+C 停止服务");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

app.Run();