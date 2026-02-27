using AgentSkillDemo.Infrastructure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step0 准备工作 — 创建 ChatClient
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();
var chatClient = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId);

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step1 创建 SkillsProvider — 从文件系统发现和加载 Skills
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var skillsProvider = new FileAgentSkillsProvider(
    skillPath: Path.Combine(Directory.GetCurrentDirectory(), "skills")
);
Console.WriteLine("📂 Skills 已从文件系统加载");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step2 创建 Agent — 注入 SkillsProvider
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "MultiSkillsAgent",
    ChatOptions = new()
    {
        Instructions = "你是一个高效的企业助手，使用和用户同样的语言回答用户提出的问题。",
    },
    AIContextProviders = [skillsProvider],
});
Console.WriteLine("✅ 多技能 Agent 创建成功");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 开始测试 — 调用Skills来回答问题
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("   🔀 测试场景：多技能智能路由");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
var session = await agent.CreateSessionAsync();
// 测试1：差旅政策问题 → 应加载 travel-policy
var travelQuestion = "我需要预订一张从纽约到伦敦、为期两周项目的航班。我可以乘坐什么舱位？需要审批吗？";
Console.WriteLine("🔵 测试1：差旅政策问题");
Console.WriteLine($"👤 用户: {travelQuestion}");
Console.WriteLine();

var travelResponse = await agent.RunAsync(travelQuestion, session);
Console.WriteLine($"🤖 Agent: {travelResponse.Text}");
Console.WriteLine();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

// 测试2：费用报销问题 → 应加载 expense-report
var expenseQuestion = "我上周买了一个 $45/月的项目管理软件订阅，需要什么审批流程？";
Console.WriteLine("🔵 测试2：费用报销问题");
Console.WriteLine($"👤 用户: {expenseQuestion}");
Console.WriteLine();

var expenseResponse = await agent.RunAsync(expenseQuestion, session);
Console.WriteLine($"🤖 Agent: {expenseResponse.Text}");
Console.WriteLine();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("👋 再见！");
Console.ReadKey();