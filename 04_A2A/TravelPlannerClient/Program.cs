using A2A;
using CommonShared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using TravelPlannerClient.Tools;

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
    .GetChatClient(openAIProvider.ModelId)
    .AsIChatClient();

// Step2. Define agent endpoints for A2A communication
var agentEndpoints = new[]
{
    "https://localhost:7021/a2a", // hotel agent 
    "https://localhost:7011/a2a", // weather agent
    "https://localhost:7031/a2a" // plan agent
};

// Step3. Collecting all AI Tools
var functionTools = new List<AIFunction>();
foreach (var endpoint in agentEndpoints)
{
    var resolver = new A2ACardResolver(new Uri(endpoint));
    var card = await resolver.GetAgentCardAsync();
    var agent = card.AsAIAgent(); // Convert A2A Agent to AIAgent instance

    functionTools.AddRange(AgentFunctionTools.CreateFunctionTools(agent, card));
}

// Step4. Create main AI Agent with Tools
var mainAgent = new ChatClientAgent(
    chatClient: chatClient,
    instructions: """
    你是一个智能旅行规划助手。你可以利用可用的工具来帮助用户完成任务。
    当用户询问时，请使用合适的工具获取信息，然后给出建议。
    """,
    tools: [.. functionTools]
   );

// 用户请求 - 测试不同的技能调用
var userRequests = new[]
{
    "查询一下上海的天气情况",
    "推荐上海的酒店",
    "帮我规划从成都到上海的旅行路线",
};

foreach (var userRequest in userRequests)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"👤 用户请求: {userRequest}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

    // 执行 Agent
    Console.WriteLine("⏱️ 主 Agent 处理中...");
    var response = await mainAgent.RunAsync(userRequest);

    Console.WriteLine($"💬 回答:\n{response.Text}");
    Console.WriteLine();
}

Console.ReadKey();