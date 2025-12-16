using CommonShared;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using SharedState.Executors;
using System.ClientModel;
using System.Text;
using System.Text.Json;

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

// Step2. Create related Agents
var fileRead = new FileReadExecutor();
var wordCounting = new WordCountingExecutor();
var paragraphCounting = new ParagraphCountingExecutor();
var aggregate = new AggregationExecutor(chatClient);

// Step3. Create a Workflow
var sharedStateWorkflow = new WorkflowBuilder(fileRead)
    .AddFanOutEdge(fileRead, [wordCounting, paragraphCounting])
    .AddFanInEdge([wordCounting, paragraphCounting], aggregate)
    .WithOutputFrom(aggregate)
    .Build();

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ Shared State Workflow 构建完成");

// Step4. Execute the Workflow
var documentKey = "ProductBrief";
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine($"📂 演示文档: {documentKey}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

await using (var run = await InProcessExecution.StreamAsync(sharedStateWorkflow, documentKey))
{
    await foreach (WorkflowEvent evt in run.WatchStreamAsync())
    {
        switch (evt)
        {
            case WorkflowStartedEvent started:
                Console.WriteLine($"🚀 Workflow Started");
                break;
            case ExecutorCompletedEvent executorCompleted:
                Console.WriteLine($"✅ {executorCompleted.ExecutorId} 完成");
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
    await run.DisposeAsync();
}

Console.ReadKey();