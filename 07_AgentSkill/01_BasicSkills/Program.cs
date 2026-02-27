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
    Name = "SingleSkillAgent",
    ChatOptions = new()
    {
        Instructions = "你是一个高效的企业助手，使用和用户同样的语言回答用户提出的问题。",
    },
    // 🔑 关键：通过 AIContextProviders 注入 SkillsProvider
    AIContextProviders = [skillsProvider],
});
Console.WriteLine("✅ Agent 创建成功");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 开始测试 — 调用Skills来回答问题
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("   📋 测试 1：费用政策 FAQ 问答");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
// 提出关于小费报销的问题
var question = "小费可以报销吗？我在一次出租车行程中给了 25% 的小费，想知道这是否可以报销吗？";
Console.WriteLine($"👤 用户: {question}");
Console.WriteLine();
// Agent 会自动执行渐进式披露流程：
// 1. 识别属于 expense-report 领域
// 2. 调用 load_skill 获取完整规则
// 3. 调用 read_skill_resource 获取 FAQ
// 4. 根据 FAQ 内容回答
var response1 = await agent.RunAsync(question);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response1.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("   📝 测试 2：多轮对话生成报销报告");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
// 使用 AgentSession 支持多轮对话
var session = await agent.CreateSessionAsync();
// 第 1 轮：提供费用信息，要求生成报告草稿
var request = "我上周有 3 笔客户晚餐费用和一张 $1,200 的机票。请先返回一份报销报告草稿，并询问我缺失的细节信息。";
Console.WriteLine($"👤 用户: {request}");
Console.WriteLine();
// Agent 会：
// 1. 加载 expense-report skill（如果尚未加载）
// 2. 读取报销模板 assets/expense-report-template.md
// 3. 根据用户提供的信息填写模板
// 4. 识别缺失字段并主动询问
var response2 = await agent.RunAsync(request, session);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response2.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
// 第 2 轮：提供详细信息，让 Agent 完善报告
var details = """
以下是详细信息：
- 客户晚餐 1：周一在 Starlight Restaurant，$280，4 人（我、来自 Marketing 的 Alice、来自 Contoso Corp 的 Bob Chen、来自 Contoso Corp 的 Lisa Wang）。业务目的：Q4 合作复盘。
- 客户晚餐 2：周三在 Golden Dragon，$195，3 人（我、来自 ABC Inc 的 Tom Li、来自 ABC Inc 的 Sarah Kim）。业务目的：新项目启动会。
- 客户晚餐 3：周五在 Café Milano，$150，2 人（我、来自 XYZ Ltd 的 David Liu）。业务目的：合同续签讨论。
- 机票：Delta Airlines，经济舱，JFK 往返 SFO，通过公司差旅平台预订。
所有收据均已附上。
""";
Console.WriteLine($"👤 用户: {details}");
Console.WriteLine();
// Agent 利用 session 中的对话历史，更新报告草稿
var response3 = await agent.RunAsync(details, session);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response3.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("👋 再见！");
Console.ReadKey();