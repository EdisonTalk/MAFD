using CommonShared;
using MapReduce.Executors.Map;
using MapReduce.Executors.Reduce;
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

// Step2. Create agents & executors
string[] summarizerIds = ["mapper_secops", "mapper_rnd", "mapper_compliance"];
var reducerId = "reducer_consensus";
var publisherId = "publisher_whitepaper";
var splitter = new ChunkSplitterExecutor(summarizerIds);
var summarizers = summarizerIds.Select(id => new DocumentSummarizerExecutor(id, reducerId)).ToArray();
var reducer = new ConsensusReducerExecutor(reducerId, publisherId);
var publisher = new DocumentPublisherExecutor(publisherId);

// Step3. Create Workflow
var workflow = new WorkflowBuilder(splitter)
    .AddFanOutEdge(splitter, [.. summarizers])
    .AddFanInEdge([.. summarizers], reducer)
    .AddEdge(reducer, publisher)
    .WithOutputFrom(publisher)
    .Build();
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ MapReduce Workflow 构建完成");

// Step4. Execute the Workflow via StreamAsync to monitor progress
string manuscript = """
第一段：本周我们在生产环境中检测到三起高危漏洞利用尝试，分别针对身份验证与API速率限制。我们已临时封禁相关IP，并更新WAF规则以缓解风险。

第二段：研发团队完成了对零信任网络访问策略的回顾，新增设备基线检查与会话时长限制，预计下周进入灰度发布阶段。

第三段：合规方面，依据最新的行业标准，我们优化了日志保留周期与隐私数据访问审批流程，减少人工审批时间。

第四段：对外联动方面，已与供应链伙伴同步威胁情报，建议对固件OTA签名策略进行交叉验证，并提升告警共享频率。
""";
Console.WriteLine("✅ 长文档样本文本已准备");
await using (var spotlightRun = await InProcessExecution.StreamAsync(workflow, manuscript))
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
                Console.WriteLine($"📦 最终摘要: {outputEvent.Data}");
                break;
        }
    }
}
Console.WriteLine("✨ MapReduce 演示结束");

Console.ReadKey();