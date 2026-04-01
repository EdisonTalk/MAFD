using AgentSkillDemo.Infrastructure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

// 本示例展示了如何完全通过代码使用 AgentInlineSkill 来定义智能体技能。
// 无需使用 SKILL.md 文件——技能、资源和脚本均通过编程方式来定义。//
// 以下是三种使用单位转换器技能的方式：
// 1. 静态资源 — 通过“添加资源”功能提供的内联内容
// 2. 动态资源 — 在运行时通过工厂委托进行计算
// 3. 代码脚本 — 可由代理直接调用的可执行委托方法

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
// Step1 代码定义Skill - Code as Skills?
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var unitConverterSkill = new AgentInlineSkill(
    name: "unit-converter",
    description: "使用乘法换算系数在常见单位之间进行转换。在需要将英里/公里、磅/千克等单位互相换算时使用。",
    instructions: """
        当用户请求单位换算时，请使用本 Skill。

        1. 查看 `conversion-table` 资源，找到正确的换算系数。
        2. 查看 `conversion-policy` 资源，了解取整和格式化规则。
        3. 使用 `convert` 脚本，并传入从表中查到的数值和系数。
        """)
    // 1. Static Resource: conversion tables
    .AddResource(
        "conversion-table",
        """
        # 换算表

        公式: **结果 = 数值 × 系数**

        | From        | To          | Factor   |
        |-------------|-------------|----------|
        | miles       | kilometers  | 1.60934  |
        | kilometers  | miles       | 0.621371 |
        | pounds      | kilograms   | 0.453592 |
        | kilograms   | pounds      | 2.20462  |
        """)
    // 2. Dynamic Resource: conversion policy (computed at runtime)
    .AddResource("conversion-policy", () =>
    {
        const int Precision = 4;
        return $"""
            # 换算策略

            **小数位数:** {Precision}
            **格式:** 始终同时显示原始值、换算后值以及单位
            **生成时间:** {DateTime.UtcNow:O}
            """;
    })
    // 3. Code Script: convert by C# code
    .AddScript("convert", (double value, double factor) =>
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    });
var skillsProvider = new AgentSkillsProvider(unitConverterSkill);
Console.WriteLine("✅ AgentSkillsProvider 创建成功");
Console.WriteLine("📂 自动注册工具: load_skill, read_skill_resource, run_skill_script");
Console.WriteLine();

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Step2 创建 Agent — 注入 SkillsProvider
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "UnitConverterAgent",
    ChatOptions = new()
    {
        Instructions = "你是一个专业的AI助手，负责帮助用户实现单位的转换。",
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
Console.WriteLine($"开始测试：基于 File-Based Skills");
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