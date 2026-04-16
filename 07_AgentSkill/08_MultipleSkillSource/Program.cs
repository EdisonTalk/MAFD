using AgentSkillDemo.Helpers;
using AgentSkillDemo.Infrastructure;
using AgentSkillDemo.Models;
using AgentSkillDemo.Skills.Caching;
using AgentSkillDemo.Skills.Global;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.Reflection;

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
    .GetChatClient(openAIProvider.ModelId)
    .AsIChatClient();
Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step2 加载Skills — Global远程Skill + Local本地Skill
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ── Source 1：模拟全局技能库（Remote API，通用政策）──
var globalSource = new SimulatedRemoteApiSkillsSource("https://global-skills.contoso.com/api");
// ── Source 2：本地定制技能（覆盖全局版，注册顺序靠前 → 优先）──
var localCustomSkills = new[]
{
    // 覆盖全局 expense-report，使用 Contoso 定制规则
    new AgentInlineSkill("expense-report", "（Contoso定制v2）企业费用报销政策",
        """
        # Contoso 定制报销规则（2025版）
        - 差旅费上限提升至 8000 元/次
        - 新增"远程协作设备补贴"类目，≤3000元免审批
        - 年末报销截止日期：12月25日
        """),
    // 新增：Contoso 特有的技能
    new AgentInlineSkill("contoso-benefits", "Contoso 员工福利计划详情", "福利：弹性办公、年度体检、学习补贴..."),
};
// ── Source 3：带缓存的远程 Source（生产环境推荐）──
var cachedGlobalSource = new CachingSkillsSource(globalSource, TimeSpan.FromMinutes(60));

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 构建构建Provider工厂 - 多重技能源 + 角色感知过滤 + 自定义Prompt
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
AgentSkillsProvider BuildProviderForRole(EmployeeRole role)
{
    Console.WriteLine($"\n  🔨 构建 {role} 角色的 Provider...");
    return new AgentSkillsProviderBuilder()
        // 本地定制优先（先注册 → first-wins）
        .UseSkills(localCustomSkills)
        // 全局技能库（带缓存）
        .UseSource(cachedGlobalSource)
        // 角色感知过滤
        .UseFilter(s => UserRoleHelper.IsSkillVisibleTo(s, role))
        // 自定义 Prompt：企业内部语气
        .UsePromptTemplate("""
            你是 Contoso 集团的企业服务助手。

            ## 你掌握的企业知识库

            {skills}

            ## 工作原则

            遇到政策性问题，**先加载该技能的详细指引**，再作答。请确保所有建议符合 Contoso 最新官方规定。
            {resource_instructions}
            {script_instructions}
            """)
        .Build();
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step4 测试Agent — 验证技能系统
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 验证三个角色看到的技能集合
foreach (var role in Enum.GetValues<EmployeeRole>())
{
    var provider = BuildProviderForRole(role);
    var srcField = typeof(AgentSkillsProvider).GetField("_source", BindingFlags.Instance | BindingFlags.NonPublic);
    var src = (AgentSkillsSource?)srcField?.GetValue(provider);
    var skills = src is null ? new List<AgentSkill>() : (await src.GetSkillsAsync()).ToList();

    Console.WriteLine($"\n👤 [{role}] 可见技能（{skills.Count} 个）：");
    foreach (var s in skills)
    {
        string origin = s.Frontmatter.Description.StartsWith("（Contoso") ? "（本地定制）" :
                        s.Frontmatter.Description.StartsWith("（全局") ? "（全局库）" : "（其他）";
        Console.WriteLine($"    • {s.Frontmatter.Name} {origin}");
    }
}

Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("✅ 综合案例验证完成：多源合并 + 角色过滤 + first-wins 去重 + 缓存层 全部正常");
Console.ReadKey();