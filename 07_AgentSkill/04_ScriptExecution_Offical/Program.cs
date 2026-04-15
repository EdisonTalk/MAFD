using AgentSkillDemo.Infrastructure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
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
Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step1 创建 SkillsProvider — 从文件系统发现和加载 Skills
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var skillsProvider = new AgentSkillsProvider(
    skillPath: Path.Combine(Directory.GetCurrentDirectory(), "skills"),
    scriptRunner: SubprocessScriptRunner.RunAsync);
Console.WriteLine("✅ AgentSkillsProvider 创建成功");
Console.WriteLine("📂 自动注册工具: load_skill, read_skill_resource, run_skill_script");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step2 创建 Agent — 注入 SkillsProvider
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "ShippingOpsAgent",
    ChatOptions = new()
    {
        Instructions = "你是一个专业的跨境物流运营助手，负责帮助用户处理相关事务，请使用用户提问的语言进行回复。",
    },
    AIContextProviders = [skillsProvider],
});
// 💡 使用 Agent Builder 注册函数调用中间件
agent = agent
    .AsBuilder()
    .Use(ToolExecutionLoggingMiddleware.ExecuteAsync) // 使用工具执行日志中间件，记录工具调用的日志
    .Build();
Console.WriteLine("✅ Agent 创建成功");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 开始测试 — 调用Skills来回答问题
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var session = await agent.CreateSessionAsync();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"开始测试：基于 File-Based Skills");
// 中文问题：英里 -> 公里
var question1 = "客户提供包裹尺寸 50x40x30 cm，实际重量 8kg，单价 12 元/kg。请按业务规则给出计费重量与预估报价。";
Console.WriteLine($"👤 用户: {question1}");
Console.WriteLine();
var response1 = await agent.RunAsync(question1, session);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response1.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("👋 再见！");
Console.ReadKey();