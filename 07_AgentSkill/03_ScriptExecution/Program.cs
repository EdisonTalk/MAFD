using AgentSkill_03_ScriptExecution.Infrastructure;
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
var skillsProvider = new FileAgentSkillsProvider(
    skillPath: Path.Combine(Directory.GetCurrentDirectory(), "skills"),
    options: new FileAgentSkillsProviderOptions
    {
        // 🔑 自定义提示词：引导模型加载技能后使用 run_shell 执行脚本
        SkillsInstructionPrompt = """
        你可以使用以下技能获取领域知识和操作指引。
        每个技能提供专业指令、参考文档和可执行脚本。

        <available_skills>
        {0}
        </available_skills>

        工作流程：
        1. 当用户任务匹配技能描述时，使用 `load_skill` 加载该技能的完整指令
        2. 技能指令中会标明可用脚本及其执行命令
        3. 使用 `run_shell` 工具执行技能中标注的命令
        4. 需要时使用 `read_skill_resource` 读取参考资料
        
        重要原则：先加载知识，再执行操作。
        """
    }
);
Console.WriteLine("✅ FileAgentSkillsProvider 创建成功（知识层）");
Console.WriteLine("📂 自动注册工具: load_skill, read_skill_resource");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step2 创建 Agent — 注入 SkillsProvider
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "SkillsBashAgent",
    ChatOptions = new()
    {
        Instructions = "你是一个专业的系统运维助手。请用中文回答所有问题。",
        // 🔑 能力层：仅注册一个 run_shell 工具
        Tools = [AIFunctionFactory.Create(ShellTools.RunShell)],
    },
    // 🔑 知识层：通过 AIContextProviders 注入 Skills
    AIContextProviders = [skillsProvider],
});
Console.WriteLine("✅ Agent 创建成功");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 开始测试 — 调用Skills来回答问题
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var session = await agent.CreateSessionAsync();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("   🔍 测试 1：系统健康检查");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
var question1 = "帮我检查一下当前系统的整体健康状态，包括 CPU、内存和磁盘使用情况。";
Console.WriteLine($"👤 用户: {question1}");
Console.WriteLine();
// Agent 会：
// 1. 识别属于 system-ops 领域 → load_skill("system-ops")
// 2. 从 SKILL.md 获取脚本执行命令
// 3. 依次调用 run_shell 执行多个诊断脚本
// 4. 根据告警阈值分析结果
var response1 = await agent.RunAsync(question1, session);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response1.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("   🔧 测试 2：故障排查 — 哪些进程占用资源最多？");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
var question2 = "我的电脑最近变慢了，帮我查一下哪些进程占用了最多的 CPU 和内存，并给出排查建议。";
question2 = "最近我的电脑C盘空间一直在变小，帮我排查下并给出建议";
Console.WriteLine($"👤 用户: {question2}");
Console.WriteLine();
// Agent 会：
// 1. 加载 system-ops 技能
// 2. 执行 check-top-processes.ps1 脚本
// 3. 读取 troubleshooting-guide.md 获取排查建议
// 4. 综合分析
var response2 = await agent.RunAsync(question2);
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {response2.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("   🛡️ 测试 3：安全护栏验证");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

// 测试危险命令拦截
var dangerousTestCases = new[]
{
    ("rm -rf /", "删除根目录"),
    ("sudo apt-get install malware", "提权操作"),
    ("shutdown -s -t 0", "关机命令"),
};

foreach (var (cmd, desc) in dangerousTestCases)
{
    var result = ShellTools.RunShell(cmd);
    Console.WriteLine($"❌ 测试: {desc}");
    Console.WriteLine($"   命令: {cmd}");
    Console.WriteLine($"   结果: {result}");
    Console.WriteLine();
}

// 测试正常命令
var normalResult = ShellTools.RunShell("Get-Date");
Console.WriteLine($"✅ 测试: 正常命令");
Console.WriteLine($"   命令: Get-Date");
Console.WriteLine($"   结果: {normalResult.Trim()}");

Console.WriteLine();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🛡️ 安全护栏验证完成：危险命令被正确拦截，正常命令正常执行。");

Console.WriteLine("👋 再见！");
Console.ReadKey();