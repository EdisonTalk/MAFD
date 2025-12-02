using CommonShared;
using CustomExecutor.Events;
using CustomExecutor.Executors;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

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

// Step2. Create 2 custom executors
var solganWriter = new SloganWriterExecutor(id: "SloganWriter", chatClient);
var feebackHandler = new FeedbackExecutor(id: "FeedbackHandler", chatClient);
Console.WriteLine("✅ Executor 实例创建完成");

// Step3. Create a workflow and register executors
var workflow = new WorkflowBuilder(solganWriter)
    .AddEdge(source: solganWriter, target: feebackHandler) // 生成 → 审核
    .AddEdge(source: feebackHandler, target: solganWriter) // 审核不通过 → 重新生成
    .WithOutputFrom(feebackHandler)                                      // 指定输出来源
    .Build();
Console.WriteLine("✅ 工作流构建完成");

// Step4. Run the workflow with an initial task
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("        智能营销文案生成与审核系统        ");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
// 定义产品任务
var productTask = "请为马自达一款经济实惠且驾驶乐趣十足的电动SUV创作标语，要求结合马自达电车的特性来创作";
Console.WriteLine($"📋 产品需求: {productTask}\n");
Console.WriteLine($"📊 审核标准: 评分 >= 8分");
Console.WriteLine($"🔄 最大尝试: 3次\n");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("⏱️ 开始执行工作流...");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
// 执行工作流
await using (var run = await InProcessExecution.StreamAsync(workflow, input: productTask))
{
    // 监听工作流事件
    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        // 使用模式匹配识别不同类型的事件
        switch (evt)
        {
            case SloganGeneratedEvent sloganEvent:
                // 处理标语生成事件
                Console.WriteLine($"✨ {sloganEvent}");
                Console.WriteLine();
                break;
            case FeedbackFinishedEvent feedbackEvent:
                // 处理审核反馈事件
                Console.WriteLine($"{feedbackEvent}");
                Console.WriteLine();
                break;
            case WorkflowOutputEvent outputEvent:
                // 处理最终输出事件
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine("🎉 工作流执行完成");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
                Console.WriteLine($"{outputEvent.Data}");
                break;
        }
    }

    Console.WriteLine("\n✅ 所有流程已完成");
}

Console.ReadKey();