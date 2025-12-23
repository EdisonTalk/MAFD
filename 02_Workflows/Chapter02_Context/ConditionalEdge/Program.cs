using CommonShared;
using ConditionalEdge.Executors;
using ConditionalEdge.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
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
var spamDetectionAgent = new ChatClientAgent(
    chatClient, 
    new ChatClientAgentOptions(instructions: "You are a spam detection assistant that labels spam emails with reasons.")
    {
        ChatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<DetectionResult>()
        }
    });
var emailAssistantAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions(instructions: "You are an enterprise email assistant. You can provide professional Chinese responses to user.")
    {
        ChatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<EmailResponse>()
        }
    });
var spamDetectionExecutor = new SpamDetectionExecutor(spamDetectionAgent);
var emailAssistantExecutor = new EmailAssistantExecutor(emailAssistantAgent);
var sendEmailExecutor = new EmailSendingExecutor();
var handleSpamExecutor = new SpamHandlingExecutor();

// Step3. Create a Workflow
Func<object?, bool> BuildCondition(bool expectedSpamFlag) =>
    detection => detection is DetectionResult dr && dr.IsSpam == expectedSpamFlag;
var conditionalWorkflow = new WorkflowBuilder(spamDetectionExecutor)
    .AddEdge(spamDetectionExecutor, emailAssistantExecutor, condition: BuildCondition(false))
    .AddEdge(emailAssistantExecutor, sendEmailExecutor)
    .AddEdge(spamDetectionExecutor, handleSpamExecutor, condition: BuildCondition(true))
    .WithOutputFrom(handleSpamExecutor, sendEmailExecutor)
    .Build();
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ Conditional Workflow 构建完成");

// Step4. Execute the Workflow                           
static async Task RunConditionalWorkflowAsync(
    Workflow conditionalWorkflow,
    string scenarioName,
    string emailContent,
    CancellationToken cancellationToken = default)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"📬 测试场景：{scenarioName}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    var chatMessage = new ChatMessage(ChatRole.User, emailContent);
    await using var run = await InProcessExecution.StreamAsync(conditionalWorkflow, chatMessage);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

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
}

// Step5. Test Case 1
var scenarioName1 = "正常咨询 → EmailAssistant 分支";
var emailContent1 = @"客服团队你好，我想确认上周提交的采购订单是否已经发货，如果还有缺少信息请告知。";
await RunConditionalWorkflowAsync(
    conditionalWorkflow,
    scenarioName1,
    emailContent1);
Console.WriteLine("✅ 正常邮件路径验证完成");

// Step6. Test Case 2
var scenarioName2 = "垃圾邮件 → HandleSpam 分支";
var emailContent2 = @"令人惊喜的投资机会！只需支付保证金即可在 24 小时内获得 10 倍收益，点击可疑链接领取奖励。";
await RunConditionalWorkflowAsync(
    conditionalWorkflow,
    scenarioName2,
    emailContent2);
Console.WriteLine("✅ 垃圾邮件路径验证完成");

Console.ReadKey();