using CommonShared;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using MultiSelectionEdge.Executors;
using MultiSelectionEdge.Models;
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
var classifier = new ContentClassifierExecutor();
var longFormExecutor = new LongFormChannelExecutor();
var shortFormExecutor = new ShortFormChannelExecutor();
var moderatorExecutor = new ModeratorSignalExecutor();

// Step3. Create Workflow
static Func<DistributionPlan?, int, IEnumerable<int>> BuildChannelRouter() => (plan, targetCount) =>
{
    if (plan is null)
    {
        return Array.Empty<int>();
    }

    List<int> targets = [];
    if (plan.PublishToLongForm) targets.Add(0);
    if (plan.PublishToShortForm) targets.Add(1);
    if (plan.EscalateToModerator) targets.Add(2);
    return targets;
};
var workflow = new WorkflowBuilder(classifier)
    .AddFanOutEdge(
        classifier,
        [longFormExecutor, shortFormExecutor, moderatorExecutor],
        BuildChannelRouter())
    .WithOutputFrom(longFormExecutor, shortFormExecutor, moderatorExecutor)
    .Build();
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ Multi-Selection Workflow 构建完成");

// Step4. Execute the Workflow via StreamAsync to monitor progress
ContentSubmission[] submissions =
{
    new("CNT-1001", "6月能耗报告", "Operations", 1200, false, "zh-CN"),
    new("CNT-1002", "618 预热短文案", "Marketing", 260, false, "zh-CN"),
    new("CNT-1003", "跨境补贴政策说明", "Compliance", 620, true, "en-US")
};
//Console.WriteLine("🧪 批量执行 3 条稿件");
//foreach (var submission in submissions)
//{
//    await using var run = await InProcessExecution.StreamAsync(workflow, submission);
//    List<string> outputs = [];

//    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
//    {
//        if (evt is WorkflowOutputEvent outputEvent)
//        {
//            outputs.Add(outputEvent.Data?.ToString() ?? string.Empty);
//        }
//    }
//}

Console.WriteLine("🔭 3条稿件的实时事件");
foreach (var submission in submissions)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"稿件内容：{submission.Title}，类别：{submission.Category}");
    await using (var spotlightRun = await InProcessExecution.StreamAsync(workflow, submission))
    {
        await foreach (WorkflowEvent evt in spotlightRun.WatchStreamAsync())
        {
            switch (evt)
            {
                case ExecutorInvokedEvent started:
                    Console.WriteLine($"🚀 {started.ExecutorId} 启动");
                    break;
                case ExecutorCompletedEvent completed:
                    Console.WriteLine($"✅ {completed.ExecutorId} 完成");
                    break;
                case WorkflowOutputEvent outputEvent:
                    Console.WriteLine($"📦 输出: {outputEvent.Data}");
                    break;
            }
        }
    }
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
}
Console.WriteLine("✨ Multi-Selection 演示结束");

Console.ReadKey();