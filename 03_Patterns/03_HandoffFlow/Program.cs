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
// 技术支持部门 - 处理产品故障、技术咨询
var technicalSupport = new ChatClientAgent(
    chatClient,
    """
    你是技术支持专家，负责解决客户的产品故障和技术问题。
    回答要点：
    1. 快速定位问题原因
    2. 提供分步骤的解决方案
    3. 如需远程协助，告知工单号和预计处理时间
    4. 只回答技术相关问题，其他问题请客户联系对应部门
    """,
    "technical_support",
    "技术支持部门 (Technical Support)");

// 财务部门 - 处理发票、账单、退款
var financeDepartment = new ChatClientAgent(
    chatClient,
    """
    你是财务部门客服，负责处理发票开具、账单查询和退款申请。
    回答要点：
    1. 核对订单号和交易信息
    2. 说明发票类型（电子/纸质）和开具时间
    3. 退款需说明到账时间（3-5个工作日）
    4. 只处理财务相关问题，其他问题请联系对应部门
    """,
    "finance_department",
    "财务部门 (Finance Department)");

// 售后服务部门 - 处理退换货、维修
var afterSalesService = new ChatClientAgent(
    chatClient,
    """
    你是售后服务专家，负责处理退换货申请、维修服务和质量投诉。
    回答要点：
    1. 确认商品是否在退换货期限内（7天无理由退货）
    2. 说明退换货流程和所需材料
    3. 维修服务需说明保修政策和预计时间
    4. 只处理售后相关问题，其他问题请联系对应部门
    """,
    "after_sales_service",
    "售后服务部门 (After-Sales Service)");

// 客服路由 - 分析问题并分配到对应部门
var customerServiceRouter = new ChatClientAgent(
    chatClient,
    """
    你是客户服务中心的智能路由系统，负责分析客户问题并转接到对应部门。
    
    判断规则：
    - 产品故障、无法开机、功能异常、技术咨询 → technical_support
    - 发票开具、账单查询、退款申请、支付问题 → finance_department  
    - 退换货、维修申请、质量投诉、配件更换 → after_sales_service
    
    重要：你必须 ALWAYS 转接到专业部门，不要自己回答问题。
    """,
    "customer_service_router",
    "客服路由 (Customer Service Router)");

Console.WriteLine("✅ 客户服务 Agent 定义完成");

// Step3. Create Workflow
// 移交规则：客户 → 路由 → [技术支持 / 财务部门 / 售后服务] → 路由 → 客户
var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(customerServiceRouter)
    .WithHandoffs(customerServiceRouter, [technicalSupport, financeDepartment, afterSalesService])
    .WithHandoffs([technicalSupport, financeDepartment, afterSalesService], customerServiceRouter)
    .Build();
Console.WriteLine("✅ 客户服务工单系统工作流构建完成");
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("✅ 工单处理流水线构建完成");

// Step4. Make a Streaming Run Helper
List<ChatMessage> conversationHistory = [];
void ResetConversation()
{
    conversationHistory.Clear();
    Console.WriteLine("🔄 对话上下文已重置");
}
Console.WriteLine("✅ 对话历史容器创建完成");
async Task AskAsync(string question)
{
    Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine($"❓ 用户：{question}");

    conversationHistory.Add(new(ChatRole.User, question));

    var finalResult = await AgentRunHelper.RunWorkflowStreamingAsync(workflow, question);
    Console.WriteLine("📊 工单的最终回复：");
    foreach (var message in finalResult)
    {
        if (message.Role != ChatRole.User && !string.IsNullOrWhiteSpace(message.Text))
        {
            Console.WriteLine(message.Text);
        }
    }
}
Console.WriteLine("✅ Streaming 运行助手已就绪");
// Step4. Execute the Workflow via AskAsync method
// 场景1：产品故障 - 技术支持
await AskAsync("你好，我购买的笔记本电脑无法开机了，按电源键完全没反应，怎么办？");
// 场景2：开具发票 - 财务部门  
await AskAsync("我需要开具增值税专用发票，订单号是 ORD20250109001，公司名称是北京科技有限公司。");
// 场景3：申请退货 - 售后服务
await AskAsync("我三天前买的手机发现屏幕有坏点，想申请退货，还在7天无理由退货期内吧？");
// 场景4：跨部门咨询 - 测试路由能力
await AskAsync("我的订单已经申请退款了，但是发票还能开吗？另外退款大概多久能到账？");
// 场景5：模糊问题 - 测试意图识别
await AskAsync("我对你们的产品质量很不满意，这个问题应该找谁解决？");

Console.ReadKey();