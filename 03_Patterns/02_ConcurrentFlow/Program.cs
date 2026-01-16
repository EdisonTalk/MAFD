using CommonShared;
using Microsoft.Agents.AI;
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
// 1. 敏感词检测 Agent（输出敏感词风险）
var sensitiveWordAgent = new ChatClientAgent(
    chatClient: chatClient,
    name: "SensitiveWordDetector",
    instructions: """
你是一位专业的内容安全审核员，负责识别文本中的敏感词汇。

检测范围：
- 政治敏感词
- 暴力、血腥内容
- 色情、低俗内容
- 歧视性言论

输出格式：
【是否包含敏感词】：是/否
【风险等级】：高/中/低/无
【具体问题】：列出发现的问题（如果有）
"""
);

// 2. 广告识别 Agent（识别推销行为）
var adDetectionAgent = new ChatClientAgent(
    chatClient: chatClient,
    name: "AdDetector",
    instructions: """
你是一位专业的广告识别专家，负责判断文本是否包含营销推广内容。

检测范围：
- 产品推广
- 引流导流（公众号、微信群等）
- 软文营销
- 联系方式（电话、邮箱、二维码）

输出格式：
【是否包含广告】：是/否
【广告类型】：产品推广/引流导流/软文营销/联系方式/无
【具体问题】：列出发现的广告内容（如果有）
"""
);

// 3. 情绪分析 Agent（评估情绪健康）
var sentimentAgent = new ChatClientAgent(
    chatClient: chatClient,
    name: "SentimentAnalyzer",
    instructions: """
你是一位专业的情绪分析专家，负责判断文本的情绪倾向。

分析维度：
- 整体情绪：正面/负面/中性
- 情绪强度：强烈/适中/平和
- 潜在影响：积极/消极/中立

输出格式：
【整体情绪】：正面/负面/中性
【情绪强度】：强烈/适中/平和
【潜在影响】：积极/消极/中立
【简要说明】：解释情绪判断的依据
"""
);
Console.WriteLine("✅ 三个评审 Agent 创建完成");

// 定义聚合逻辑
Func<IList<List<ChatMessage>>, List<ChatMessage>> auditAggregator = (agentResults) =>
{
    var sb = new StringBuilder();
    sb.AppendLine("# 📝 博客质量评审报告");
    sb.AppendLine($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━");

    // 建立 Agent 名称映射
    var agentNameMap = new Dictionary<string, string>
    {
        { "SensitiveWordDetector", "🔍 敏感词检测" },
        { "AdDetector", "📢 广告识别" },
        { "SentimentAnalyzer", "😊 情绪分析" }
    };

    // 由于并发执行顺序可能不确定，这里直接遍历所有结果进行拼接
    foreach (var history in agentResults)
    {
        var lastMessage = history.LastOrDefault();
        if (lastMessage != null)
        {
            // 获取 Agent 名称（如果有）
            var agentName = lastMessage.AuthorName ?? "评审专家";

            // 尝试获取友好的显示名称
            if (agentNameMap.TryGetValue(agentName, out var displayName))
            {
                agentName = displayName;
            }

            sb.AppendLine($"## {agentName}");
            sb.AppendLine(lastMessage.Text);
            sb.AppendLine();
        }
    }

    return new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, sb.ToString()) };
};
Console.WriteLine("✅ 聚合逻辑创建完成");

// Step3. Create Workflow
// 构建并发工作流
var workflow = AgentWorkflowBuilder.BuildConcurrent(
    agents: new[] { sensitiveWordAgent, adDetectionAgent, sentimentAgent },
    aggregator: auditAggregator
);
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ 工单处理流水线构建完成");

// Step4. Execute the Workflow via StreamAsync to monitor progress
// 模拟博客内容
var blogContent = """
# 我的AI学习之旅

最近我开始学习人工智能技术，感觉收获特别大！分享一些心得体会。

首先，选择一个好的学习平台很重要。我现在在某某教育平台学习（文末有优惠码），
课程质量真的不错，推荐大家也试试。加我微信：abc123，可以拉你进学习群。

其次，要多实践。光看理论不够，要动手写代码。我已经完成了10个小项目，
感觉自己的技术水平突飞猛进！

希望大家都能在AI领域有所收获！记得关注我的公众号：XXX技术分享，持续更新干货！
""";

Console.WriteLine("📝 待审核的博客内容：");
Console.WriteLine(blogContent);
Console.WriteLine();
Console.WriteLine("⏱️  开始并发评审...");
try
{
    List<ChatMessage> request = new()
    {
        new ChatMessage(ChatRole.User, blogContent)
    };

    // 执行工作流
    // 注意：RunWorkflowStreamingAsync 会流式输出过程，最后返回结果
    var finalResult = await AgentRunHelper.RunWorkflowStreamingAsync(workflow, blogContent);

    Console.WriteLine();
    Console.WriteLine("📊 聚合后的最终报告：");
    foreach (var message in finalResult)
    {
        Console.WriteLine(message.Text);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 执行失败：{ex.Message}");
}

Console.ReadKey();