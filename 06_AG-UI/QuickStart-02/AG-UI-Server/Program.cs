using AgUIBackend.Infrastructure;
using AgUIBackend.Models;
using AgUIBackend.Tools;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;

// Step0. 加载配置文件
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();
var chatClient = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId);

// Step1. 做好准备工作
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
// ⭐ 配置 JSON 序列化上下文 (用于数据模型的序列化)
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Add(MixedToolsJsonContext.Default));

// Step2. Register AG-UI services
builder.Services.AddAGUI();

var app = builder.Build();

// Step3. Create AI Agent
var jsonOptions = app.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
AITool[] backendTools =
[
    AIFunctionFactory.Create(BackendTools.SearchNearbyRestaurants, serializerOptions: jsonOptions.SerializerOptions),
    AIFunctionFactory.Create(BackendTools.GetRestaurantDetail, serializerOptions: jsonOptions.SerializerOptions)
];
Console.WriteLine("🔧 已注册后端工具：");
foreach (var tool in backendTools)
{
    Console.WriteLine($"   • {tool.Name} (服务端执行)");
}
Console.WriteLine("📝 前端工具将由客户端注册");

var agent = chatClient.AsIChatClient()
    .AsAIAgent(
        name: "MixedToolsAssistant",
    instructions: """
        你是一个智能餐厅推荐助手，具备以下能力：
        
        🌍 位置感知：
        - 可以获取用户的当前位置（使用 GetUserLocation 工具）
        - 当用户说"附近"、"周围"时，先获取位置
        
        🍽️ 餐厅推荐：
        - 使用 SearchNearbyRestaurants 搜索附近餐厅
        - 使用 GetRestaurantDetail 获取详细信息
        
        🎯 使用流程：
        1. 如果用户问"附近有什么餐厅"，先调用 GetUserLocation 获取位置
        2. 然后调用 SearchNearbyRestaurants 搜索
        3. 如果用户想了解某家餐厅，调用 GetRestaurantDetail
        
        请用中文友好地回答用户。
        """,
    tools: backendTools);
Console.WriteLine("✅ AI Agent 创建成功（混合工具模式）");

// 映射 AG-UI 端点
app.MapAGUI("/", agent);

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🚀 AG-UI Server (Mixed Tools) 已启动");
Console.WriteLine("📍 端点地址: https://localhost:8443/");
Console.WriteLine("🔧 后端工具: SearchNearbyRestaurants, GetRestaurantDetail");
Console.WriteLine("📱 前端工具: GetUserLocation (客户端定义)");
Console.WriteLine("💡 使用 Ctrl+C 停止服务");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

app.Run();