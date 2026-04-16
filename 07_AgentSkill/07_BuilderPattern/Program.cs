using AgentSkillDemo.ClassSkills;
using AgentSkillDemo.Infrastructure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

/*
 * - 距离/重量换算（文件型 Skill）
 * - 运费估算与费率解释（代码型 Skill）
 * - 包装建议与成本估算（类定义 Skill）
 */

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

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step2 创建 Agent — 注入 SkillsProvider
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var freightCostSkill = new AgentInlineSkill(
    name: "freight-cost-estimator",
    description: "根据计费重量与线路信息估算跨境物流运费。",
    instructions: """
        当用户需要运费估算时使用本技能。

        1. 先读取 tariff-rate-table 资源确定线路基准单价与附加费率。
        2. 再调用 estimate-freight 脚本进行计算。
        3. 回复中必须给出输入参数、计算过程、最终价格。
        """)
    .AddResource("tariff-rate-table", """
        # 线路费率表（示例）

        | route | basePricePerKg(CNY) | fuelSurchargeRate |
        | --- | ---: | ---: |
        | CN-DE | 41.5 | 0.12 |
        | CN-US | 38.0 | 0.10 |
        | CN-JP | 26.0 | 0.08 |

        汇率说明：
        - USD/CNY = 7.25
        - EUR/CNY = 7.86
        """)
    .AddScript("estimate-freight", (double chargeableWeight, double basePricePerKg, double fuelSurchargeRate) =>
    {
        double baseCost = Math.Round(chargeableWeight * basePricePerKg, 2);
        double fuelCost = Math.Round(baseCost * fuelSurchargeRate, 2);
        double total = Math.Round(baseCost + fuelCost, 2);

        return JsonSerializer.Serialize(new
        {
            chargeableWeight,
            basePricePerKg,
            fuelSurchargeRate,
            baseCost,
            fuelCost,
            total
        });
    });
var packagingAdvisorSkill = new PackagingAdvisorSkill();
var logisticsPromptTemplate = """
你是 Contoso 公司跨境物流运营助手，请遵循以下规则：

1. 优先给出可执行建议，不要只给概念解释。
2. 对任何报价结果都要显示输入、公式、结论。
3. 涉及脚本执行时，先说明原因再执行。

## 可用技能
{skills}

## 资源读取约束
{resource_instructions}

## 脚本调用约束
{script_instructions}
""";
var skillsProvider = new AgentSkillsProviderBuilder()
    .UseFileSkill("FileSkills") 
    .UseSkill(freightCostSkill) // Inline Skill
    .UseSkill(packagingAdvisorSkill) // Class-Based Skill
    .UsePromptTemplate(logisticsPromptTemplate)
    .UseFileScriptRunner(AgentHelper.RunScriptInSubProcessAsync)
    .UseScriptApproval(true) // Enbale script approval to ensure safety when executing scripts
    .Build();
var skillsContext = await AgentHelper.GetAgentSkillsContextAsync(chatClient, skillsProvider);
var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "ContosoLogisticsAgent",
    ChatOptions = new()
    {
        Instructions = "你是Contoso公司跨境物流运营助手，请优先调用可用 skills 来完成换算、估价与包装建议。"
    },
    AIContextProviders = [skillsProvider],
});
Console.WriteLine("✅ 三种类型Skills统一组装的 AI Agent 创建成功");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step3 开始测试 — 调用Skills来回答问题
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var session = await agent.CreateSessionAsync();
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
var question = "请帮我完成三件事：26.2 英里换算公里；50 磅货物按 41.5 元/kg、燃油附加 12% 估算运费；给 fragile 类别提供包装建议并估算 3 件包装成本。";
Console.WriteLine($"👤 用户: {question}");
Console.WriteLine();
var response = await agent.RunAsync(question, session);

var approvalRequestContents = response.Messages
        .SelectMany(m => m.Contents)
        .OfType<ToolApprovalRequestContent>()
        .ToList();

var userResponses = new List<ChatMessage>();
foreach (var approvalRequest in approvalRequestContents)
{
    var aiFunctionCall = approvalRequest.ToolCall as FunctionCallContent;
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"⚠️ 你有一个脚本执行的审批请求：");
    Console.WriteLine($"   函数: {aiFunctionCall?.Name}");
    Console.WriteLine($"   参数: {aiFunctionCall?.Arguments}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    Console.WriteLine($"是否允许执行? (Y/N): ");
    var decision = Console.ReadLine();
    var approved = decision.Equals("Y", StringComparison.OrdinalIgnoreCase);

    var approvalResponse = approvalRequest.CreateResponse(approved);
    userResponses.Add(new ChatMessage(ChatRole.User, [approvalResponse]));
}

// 返回审批结果
var finalResponse = await agent.RunAsync(userResponses, session);

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"🤖 Agent: {finalResponse.Text}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

Console.WriteLine("👋 再见！");
Console.ReadKey();