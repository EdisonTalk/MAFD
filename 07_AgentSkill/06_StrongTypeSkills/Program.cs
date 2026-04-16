using AgentSkillDemo.Infrastructure;
using AgentSkillDemo.Middlewares;
using AgentSkillDemo.Skills;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step1 准备工作 — 创建 ChatClient
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
// Step2 创建 Agent — 注入 SkillsProvider
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var skillsProvider = new AgentSkillsProvider(new UnitConverterSkill());
AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "UnitConverterAgent",
    ChatOptions = new()
    {
        Instructions = "你是一个专业的AI助手，负责帮助用户实现单位的转换，使用用户提问的语言进行回复。",
    },
    AIContextProviders = [skillsProvider],
});
// 💡 使用 Agent Builder 注册函数调用中间件
agent = agent
    .AsBuilder()
    .Use(ToolExecutionLoggingMiddleware.ExecuteAsync) // 使用工具执行日志中间件，记录工具调用的日志
    .Build();
Console.WriteLine("✅ 基于强类型Skills 的 AI Agent 创建成功");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 开始测试 — 调用Skills来回答问题
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var session = await agent.CreateSessionAsync();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"开始测试：基于 Class-Based Skills");
// 中文问题：英里 -> 公里
var question1 = "马拉松比赛的距离26.2 英里是多少公里？";
Console.WriteLine($"👤 用户: {question1}");
Console.WriteLine();
var response1 = await agent.RunAsync(question1, session);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response1.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
// 英文问题：磅 -> 千克
var question2 = "How many pounds is 75 kilograms?";
Console.WriteLine($"👤 用户: {question2}");
Console.WriteLine();
var response2 = await agent.RunAsync(question2, session);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response2.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("👋 再见！");
Console.ReadKey();