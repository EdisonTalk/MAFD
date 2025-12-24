using CommonShared;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using SwitchCaseV1.Executors;
using SwitchCaseV1.Models;
using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;

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

// Step2. Create related Agents & Executors
// Agents
var spamDetectionAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions(
        instructions: @"你是一个垃圾邮件检测助手。判定规则：
      
- NotSpam: 明显的正常业务邮件（订单查询、售后咨询等）
- Spam: 明显的垃圾邮件（诈骗、广告、钓鱼）
- Uncertain: 无法明确判断，包含可疑元素但不确定（如含可疑链接但内容模糊）

对于模棱两可的情况，倾向于标记为 Uncertain 以保证安全。"
    )
    {
        ChatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<DetectionResult>()
        }
    }
);
var emailAssistantAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions(
        instructions: "你是一个企业邮件助手，为客户邮件生成专业、友好的中文回复。"
    )
    {
        ChatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<EmailResponse>()
        }
    }
);
// Executors
var spamDetectionExecutor = new SpamDetectionExecutor(spamDetectionAgent);
var emailAssistantExecutor = new EmailAssistantExecutor(emailAssistantAgent);
var sendEmailExecutor = new EmailSendingExecutor();
var handleSpamExecutor = new SpamHandlingExecutor();
var handleUncertainExecutor = new UncertainHandlingExecutor();

// Step3. Create a Workflow
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 🔧 条件函数工厂方法
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Func<object?, bool> GetCondition(SpamDecision expectedDecision) =>
    detectionResult =>
        detectionResult is DetectionResult result &&
        result.spamDecision == expectedDecision;
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 🔀 使用 AddSwitch 构建 Switch-Case 工作流
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
var builder = new WorkflowBuilder(spamDetectionExecutor);
builder.AddSwitch(spamDetectionExecutor, sb =>
        sb
            // Case 1: NotSpam → EmailAssistant 
            .AddCase(GetCondition(expectedDecision: SpamDecision.NotSpam), new[] { (ExecutorBinding)emailAssistantExecutor })
            // Case 2: Spam → HandleSpam
            .AddCase(GetCondition(expectedDecision: SpamDecision.Spam), new[] { (ExecutorBinding)handleSpamExecutor })
            // Default: Uncertain (或任何未匹配的情况) → HandleUncertain
            .WithDefault(new[] { (ExecutorBinding)handleUncertainExecutor })
    )
    // EmailAssistant 之后自动发送邮件
    .AddEdge(emailAssistantExecutor, sendEmailExecutor)
    // 配置输出节点（三个终点执行器都会产生输出）
    .WithOutputFrom(handleSpamExecutor, sendEmailExecutor, handleUncertainExecutor);
var workflow = builder.Build();
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ Switch-Case Workflow 构建完成");

// Step4. Execute the Workflow
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 🔄 通用工作流运行函数
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
static async Task RunWorkflowAsync(
    Workflow workflow,
    string scenarioName,
    string emailContent)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"📬 测试场景：{scenarioName}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"📧 邮件内容：{emailContent.Substring(0, Math.Min(80, emailContent.Length))}...\n");

    await using var run = await InProcessExecution.StreamAsync(
        workflow,
        new ChatMessage(ChatRole.User, emailContent)
    );

    // 发送 Turn Token，启用事件推送
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    // 订阅事件流
    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        switch (evt)
        {
            case ExecutorCompletedEvent completedEvent:
                Console.WriteLine($"✅ {completedEvent.ExecutorId} 完成");
                break;
            case WorkflowOutputEvent outputEvent:
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine("🎉 工作流执行完成");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                Console.WriteLine($"{outputEvent.Data}");
                break;
            case WorkflowErrorEvent errorEvent:
                Console.WriteLine("✨ 收到 Workflow Error Event：");
                Console.WriteLine($"{errorEvent.Data}");
                break;
            default:
                break;
        }
    }

    Console.WriteLine();
}

// Step5. Test Case 1
var scenarioName1 = "正常邮件 → EmailAssistant → SendEmail";
var emailContent1 = @"
尊敬的客服团队：

您好！我是贵公司的长期客户，订单号为 #2025-001。

我想确认一下上周提交的采购订单是否已经安排发货。
如果需要补充任何信息，请随时告知。

期待您的回复，谢谢！

客户：张先生
";
await RunWorkflowAsync(workflow, scenarioName1, emailContent1);
Console.WriteLine("✅ 正常邮件路径验证完成");

// Step6. Test Case 2
var scenarioName2 = "垃圾邮件 → HandleSpam";
var emailContent2 = @"
🎉🎉🎉 恭喜您中奖啦！🎉🎉🎉

您已被选中获得 100 万现金大奖！

立即点击以下链接领取：
http://suspicious-site.com/claim-prize

仅限今日有效，过期作废！
不需要任何手续费，完全免费！

快速行动，机不可失！
";
await RunWorkflowAsync(workflow, scenarioName2, emailContent2);
Console.WriteLine("✅ 垃圾邮件路径验证完成");

// Step7. Test Case 3
var uncertainEmail = @"
主题：需要验证您的账户

尊敬的客户：

我们检测到您的账户存在异常活动，需要验证您的身份以确保账户安全。

请登录您的账户并完成验证流程，以继续使用服务。

账户详情：
- 用户名：johndoe@contoso.com
- 最后登录：08/15/2025
- 登录地点：西雅图，华盛顿州
- 登录设备：移动设备

这是一项自动安全措施。如果您认为此邮件是错误发送的，请立即联系我们的支持团队。

此致
安全团队
客户服务部门
";
await RunWorkflowAsync(
    workflow,
    "不确定邮件 → HandleUncertain (Default)",
    uncertainEmail
);
Console.WriteLine("✅ 不确定邮件路径验证完成");

Console.ReadKey();