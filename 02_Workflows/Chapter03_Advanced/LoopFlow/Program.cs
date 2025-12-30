using CommonShared;
using LoopFlow.Constants;
using LoopFlow.Events;
using LoopFlow.Executors;
using LoopFlow.Models.ValueObjects;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
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

// Step2. Create some sample data and const values
var ticketRequests = new[]
{
    //new TicketRequest("TKT-2025-001", "我买的手机充不进电了，什么情况？", "电子产品", "High"),
    //new TicketRequest("TKT-2025-002", "订单已经3天没发货，能退款吗？", "物流问题", "Medium"),
    new TicketRequest("TKT-2025-003", "会员积分为什么突然清零了？", "账户问题", "Low")
};
const int maxAttempts = 5;

// Step3. Create Workflows
var workflows = new List<Workflow>();
foreach (var ticket in ticketRequests)
{
    Console.WriteLine($"🎫 用户已提交工单请求：{ticket.Id} - {ticket.Query}");
    // baseline
    //var draftExecutor = new BaselineReplyDraftExecutor(ticket, chatClient);
    //var qcExecutor = new BaselineQualityCheckExecutor(QualityCheckConstants.PolitenessThreshold, QualityCheckConstants.AccuracyThreshold, chatClient);
    //var workflow = new WorkflowBuilder(draftExecutor)
    //    .AddEdge(draftExecutor, qcExecutor)
    //    .AddEdge(qcExecutor, draftExecutor)
    //    .WithOutputFrom(qcExecutor)
    //    .Build();
    // adaptive
    var adaptiveDraft = new AdaptiveReplyDraftExecutor(ticket, chatClient);
    var adaptiveQC = new AdaptiveQualityCheckExecutor(QualityCheckConstants.PolitenessThreshold, QualityCheckConstants.AccuracyThreshold, chatClient);
    var intelligentImprove = new IntelligentImproveExecutor(ticket, chatClient);
    var workflow = new WorkflowBuilder(adaptiveDraft)
        .AddEdge(adaptiveDraft, adaptiveQC)
        .AddEdge(adaptiveQC, intelligentImprove)
        .AddEdge(intelligentImprove, adaptiveDraft)
        .WithOutputFrom(adaptiveQC)
        .Build();

    workflows.Add(workflow);
}
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ Loop Workflow 构建完成");

// Step4. Execute the Workflow
foreach (var workflow in workflows)
{
    await using (var run = await InProcessExecution.StreamAsync(workflow, QualityCheckSignal.Init))
    {
        var scoreTimeline = new List<object>();

        await foreach (var evt in run.WatchStreamAsync())
        {
            // 强制中断（最多5次尝试）
            if (scoreTimeline.Count == maxAttempts)
            {
                Console.WriteLine($"⛔ 强制中断工作流执行（已完成{maxAttempts}次评估）");
                break;
            }

            switch (evt)
            {
                // baseline
                //case BaselineQualityScoreEvent scoreEvent:
                //    dynamic payload = scoreEvent.Data!;
                //    scoreTimeline.Add(new QualityScoreTimelineItem(
                //        Attempt: (int)payload.Attempt,
                //        PolitenessScore: (int)payload.PolitenessScore,
                //        AccuracyScore: (int)payload.AccuracyScore,
                //        ComplianceStatus: (bool)payload.CompliancePassed ? "✅" : "❌"
                //    ));
                //    var statusMessage = payload.CompliancePassed ? "通过" : "不通过";
                //    Console.WriteLine($"📊 AI 质检结果 => 礼貌度:{payload.PolitenessScore} 准确性:{payload.AccuracyScore} 合规性:{statusMessage}");
                //    break;
                // adaptive
                case AdaptiveQualityScoreEvent scoreEvent:
                    dynamic payload = scoreEvent.Data!;
                    scoreTimeline.Add(new QualityScoreTimelineItem(
                        Attempt: (int)payload.Attempt,
                        PolitenessScore: (int)payload.PolitenessScore,
                        AccuracyScore: (int)payload.AccuracyScore,
                        ComplianceStatus: (bool)payload.CompliancePassed ? "✅" : "❌"
                    ));
                    var statusMessage = payload.CompliancePassed ? "通过" : "不通过";
                    Console.WriteLine($"📊 AI 质检结果 => 礼貌度:{payload.PolitenessScore} 准确性:{payload.AccuracyScore} 合规性:{statusMessage}");
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
            }
        }
    }
}

Console.ReadKey();