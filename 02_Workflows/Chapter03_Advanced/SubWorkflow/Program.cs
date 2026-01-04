using CommonShared;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using SubWorkflow.Executors;
using SubWorkflow.Executors.CustomPortal;
using SubWorkflow.Executors.ProductLogisticsDomain;
using SubWorkflow.Executors.ProductQualityDomain;
using SubWorkflow.Models;
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
Console.WriteLine("✅ AI 客户端初始化完成");

// Step2. Create Sub Workflows on Product Quality Domain
var productEvalExecutor = new ProductEvaluationExecutor();
var returnPolicyExecutor = new ReturnPolicyExecutor();
var aiResponseExecutor = new AIResponseGeneratorExecutor(chatClient);

var productQualitySubWorkflow = new WorkflowBuilder(productEvalExecutor)
    .AddEdge(productEvalExecutor, returnPolicyExecutor)
    .AddEdge(returnPolicyExecutor, aiResponseExecutor)
    .WithOutputFrom(aiResponseExecutor)
    .Build();
var productQualitySubExecutor = productQualitySubWorkflow.BindAsExecutor("ProductQualitySubWorkflow");

// Step3. Create Sub Workflows on Product Logistics Domain
var logisticsTrackExecutor = new LogisticsTrackingExecutor();
var delayAnalysisExecutor = new DelayAnalysisExecutor();
var logisticsAIResponseExecutor = new AIResponseGeneratorExecutor(chatClient);

var logisticsSubWorkflow = new WorkflowBuilder(logisticsTrackExecutor)
    .AddEdge(logisticsTrackExecutor, delayAnalysisExecutor)
    .AddEdge(delayAnalysisExecutor, logisticsAIResponseExecutor)
    .WithOutputFrom(logisticsAIResponseExecutor)
    .Build();
var logisticsSubExecutor = logisticsSubWorkflow.BindAsExecutor("LogisticsSubWorkflow");

// Step3. Create Main Workflow
var classifierExecutor = new ComplaintClassifierExecutor();
var complianceExecutor = new ComplianceCheckExecutor();
var sentimentExecutor = new SentimentAnalysisExecutor(chatClient);

// 主工作流：分类 → 条件路由到子流程 → 合规 → 情绪
var mainWorkflow = new WorkflowBuilder(classifierExecutor)
    .AddEdge<ComplaintProcessingRecord>(classifierExecutor, productQualitySubExecutor,
        condition: record => record.Category == "产品质量")
    .AddEdge<ComplaintProcessingRecord>(classifierExecutor, logisticsSubExecutor,
        condition: record => record.Category == "物流问题")
    .AddEdge(productQualitySubExecutor, complianceExecutor)
    .AddEdge(logisticsSubExecutor, complianceExecutor)
    .AddEdge(complianceExecutor, sentimentExecutor)
    .WithOutputFrom(sentimentExecutor)
    .Build();

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("🏗️ 主工作流构建完成（包含条件路由）");

// Step4. Execute the Workflow via StreamAsync to monitor progress
var processingRecord = new ComplaintProcessingRecord 
{ 
    Original = new CustomerComplaint(
        OrderId: "ORD-2025-8821",
        CustomerName: "张先生",
        ComplaintText: "收到的手机屏幕有明显划痕，要求退货退款",
        SubmittedAt: DateTime.Now)
};
Console.WriteLine("🚀 开始执行投诉处理流水线...");
Console.WriteLine(new string('━', 60));
await using (var streaming = await InProcessExecution.StreamAsync(mainWorkflow, processingRecord))
{
    await foreach (WorkflowEvent evt in streaming.WatchStreamAsync())
    {
        switch (evt)
        {
            case ExecutorInvokedEvent started:
                Console.WriteLine($"\n🔹 {started.ExecutorId} 开始执行");
                break;
            case ExecutorCompletedEvent completed when completed.Data is ComplaintProcessingRecord rec:
                Console.WriteLine($"   共享状态更新 → 处理步骤数：{rec.ProcessingSteps.Count}");
                break;
            case WorkflowOutputEvent outputEvt when outputEvt.Data is ComplaintProcessingRecord finalRecord:
                Console.WriteLine("\n" + new string('━', 60));
                Console.WriteLine("🎉 投诉处理完成！最终处理记录:\n");
                Console.WriteLine(JsonSerializer.Serialize(finalRecord));
                Console.WriteLine("\n📝 处理步骤详情：");
                foreach (var step in finalRecord.ProcessingSteps)
                {
                    Console.WriteLine($"  {step}");
                }

                Console.WriteLine("\n💬 AI 生成的客户回复：");
                Console.WriteLine(new string('─', 60));
                Console.WriteLine(finalRecord.AIGeneratedResponse);
                Console.WriteLine(new string('─', 60));
                break;
        }
    }
}

Console.ReadKey();