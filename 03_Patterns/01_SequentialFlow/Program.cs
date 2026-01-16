using CommonShared;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using SequentialFlow.Factories;
using System.ClientModel;
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
Console.WriteLine("✅ AI 客户端初始化完成");

// Step2. Create agents & executors
// Agent 1: 负责分析情绪和分类
var triageAgent = CustomAgentFactory.CreateAgent(
    "TriageSpecialist",
    "Customer Support Triage Specialist. Analyze the customer's message. Identify the **Sentiment** (e.g., Angry, Frustrated, Neutral), **Issue Category** (e.g., Billing, Technical, Feature Request), and **Urgency**. Output a structured summary.",
    chatClient);
// Agent 2: 负责制定解决方案
var solutionAgent = CustomAgentFactory.CreateAgent(
    "SolutionSpecialist",
    "Senior Support Specialist. Based on the triage summary, provide a specific resolution plan or policy explanation. Do not draft the final email yet, just list the key points and actions to be taken (e.g., issue refund, schedule technician).",
    chatClient);
// Agent 3: 负责撰写回复
var replyAgent = CustomAgentFactory.CreateAgent(
    "CommunicationManager",
    "Customer Relations Manager. Draft a polite, empathetic, and professional response to the customer. Incorporate the resolution plan provided by the Specialist. Adjust your tone based on the customer's initial sentiment (e.g., be extra apologetic if they were angry).",
    chatClient);
Console.WriteLine("✅ 客服工单处理团队 Agent 创建完成");

// Step3. Create Workflow
// 使用 AgentWorkflowBuilder 构建顺序工作流
var workflow = AgentWorkflowBuilder.BuildSequential(
    workflowName: "CustomerTicketPipeline",
    agents: [triageAgent, solutionAgent, replyAgent]
);
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ 工单处理流水线构建完成");

// Step4. Execute the Workflow via StreamAsync to monitor progress
var customerComplaint = "我真的很生气！上周我就付了年度会员费（订单号 #9981），但是到现在我的账号还是显示普通用户！我已经给你们发了两封邮件了都没人回。如果今天不解决，我就要退款并投诉你们！";
Console.WriteLine($"📝 客户投诉内容:\n{customerComplaint}\n");
Console.WriteLine("🚀 流水线启动...\n");
await using (StreamingRun run = await InProcessExecution.StreamAsync(workflow, customerComplaint))
{
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true)); // Enable event emitting

    var result = new List<ChatMessage>();
    var stageOutput = new StringBuilder();
    int stepNumber = 1;
    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        if (evt is AgentRunUpdateEvent updatedEvent)
        {
            stageOutput.Append($"{updatedEvent.Data} ");
        }
        else if (evt is ExecutorCompletedEvent completedEvent)
        {
            if (stageOutput.Length > 0)
            {
                Console.WriteLine($"Step {stepNumber}: {completedEvent.ExecutorId}");
                Console.WriteLine($"Output: {stageOutput.ToString()}\n");
                stepNumber++;
                stageOutput.Clear();
            }
        }
        else if (evt is WorkflowOutputEvent endEvent)
        {
            result = (List<ChatMessage>)endEvent.Data!;
            break;
        }
    }

    // Display final result
    foreach (var message in result.Skip(1))
        Console.WriteLine($"Agent: {message.Text}");
}
Console.WriteLine("\n\n✅ 工单处理完毕");

Console.ReadKey();