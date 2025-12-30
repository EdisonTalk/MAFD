using CommonShared;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using ParallelExecution.Executors;
using ParallelExecution.Models;
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

// Step2. Create agents
var amazonExecutor = new PlatformPriceExecutor(
    "AmazonPriceAgent",
    chatClient,
    "你是Amazon平台价格查询Agent。返回格式：价格=$XXX，库存状态=充足/紧张，配送说明=Prime会员免运费/标准配送。"
);
var ebayExecutor = new PlatformPriceExecutor(
    "eBayPriceAgent",
    chatClient,
    "你是eBay平台价格查询Agent。返回格式：价格=$XXX，商品状态=全新/二手XX新，运费说明=包邮/买家承担。"
);
var shopeeExecutor = new PlatformPriceExecutor(
    "ShopeePriceAgent",
    chatClient,
    "你是Shopee平台价格查询Agent。返回格式：价格=$XXX（含税），区域=东南亚/台湾，促销信息=满减活动/无。"
);
var startExecutor = new PriceQueryStartExecutor();
var strategyExecutor = new PricingStrategyExecutor(3);

// Step3. Create some sample data
var priceQuery = new PriceQueryDto(
    productId: "IPHONE15-PRO-256",
    productName: "iPhone 15 Pro 256GB",
    targetRegion: "US"
);

// Step4. Create Workflow
var workflow = new WorkflowBuilder(startExecutor)
        .AddFanOutEdge(startExecutor, [amazonExecutor, ebayExecutor, shopeeExecutor])
        .AddFanInEdge([amazonExecutor, ebayExecutor, shopeeExecutor], strategyExecutor)
        .WithOutputFrom(strategyExecutor)
        .Build();
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ Fan-out/Fan-in Workflow 构建完成");

// Step4. Execute the Workflow via StreamAsync to monitor progress
await using (var run = await InProcessExecution.StreamAsync(workflow, priceQuery))
{
    await foreach (var evt in run.WatchStreamAsync())
    {
        switch (evt)
        {
            case ExecutorInvokedEvent started:
                Console.WriteLine($"🚀 {started.ExecutorId} 开始运行");
                break;
            case ExecutorCompletedEvent completed:
                Console.WriteLine($"✅ {completed.ExecutorId} 结束运行");
                break;
            case WorkflowOutputEvent outputEvent:
                Console.WriteLine("🎉 Fan-in 汇总输出：");
                Console.WriteLine($"{outputEvent.Data}");
                break;
            case WorkflowErrorEvent errorEvent:
                Console.WriteLine("✨ 收到 Workflow Error Event：");
                Console.WriteLine($"{errorEvent.Data}");
                break;
        }
    }
}

Console.ReadKey();