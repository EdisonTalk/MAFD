using AgUIFrontend.Infrastructure;
using AgUIFrontend.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Threading;

// 读取配置文件
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();
var serverEndpoint = config.GetValue<string>("AGUI_SERVER_URL")
    ?? "https://localhost:8443";

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🚀 AG-UI 客户端已启动");
Console.WriteLine($"📍 服务端地址: {serverEndpoint}");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

// 加载前端工具
AITool[] frontendTools =
[
    AIFunctionFactory.Create(FrontendTools.GetUserLocation),
    AIFunctionFactory.Create(FrontendTools.GetUserPreferences)
];
Console.WriteLine("🔧 已注册前端工具：");
foreach (var tool in frontendTools)
{
    Console.WriteLine($"   • {tool.Name} (客户端执行)");
}
Console.WriteLine("📝 后端工具由服务端提供 (SearchNearbyRestaurants, GetRestaurantDetail)");
Console.WriteLine();

// Step1. Create HTTP Client
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(180) // 延长超时，因为可能有多个工具调用
};

// Step2. Create AG-UI Client
var chatClient = new AGUIChatClient(httpClient, serverEndpoint);

// Step3. Create AI Agent
var agent = chatClient.AsAIAgent(
    name: "agui-client",
    description: "AG-UI 混合工具客户端",
    tools: frontendTools);  // 👈 注册前端工具

// Step4. Prepare for Conversation
var session = await agent.GetNewSessionAsync();
var messages = new List<ChatMessage>()
{
    new ChatMessage(ChatRole.System, """
        你是一个智能餐厅推荐助手。
        你可以使用多种工具来帮助用户找到合适的餐厅。
        当用户问"附近有什么餐厅"时，先获取他们的位置，再搜索餐厅。
        """)
};

Console.WriteLine("💬 开始对话（输入 :q 或 quit 退出）\n");

while (true)
{
    Console.Write("👤 用户: ");
    string? message = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(message))
    {
        Console.WriteLine("⚠️ 消息不能为空，请重新输入。");
        continue;
    }

    if (message is ":q" or "quit")
    {
        Console.WriteLine("\n👋 再见！");
        break;
    }


    // 添加用户消息
    messages.Add(new ChatMessage(ChatRole.User, message));

    // 统计工具调用
    int frontendToolCalls = 0;
    int backendToolCalls = 0;
    bool isFirstUpdate = true;
    List<string> toolCallChain = [];

    Console.WriteLine();
    Console.WriteLine("━━━━━━━━━━━━━━ 开始处理 ━━━━━━━━━━━━━━━");

    // 流式接收响应
    Console.Write("🤖 助手: ");
    await foreach (var update in agent.RunStreamingAsync(messages, session))
    {
        var chatUpdate = update.AsChatResponseUpdate();

        if (isFirstUpdate)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[🔄 Run Started - Thread: {chatUpdate.ConversationId?.Substring(0, 8)}...]");
            Console.ResetColor();
            isFirstUpdate = false;
        }

        foreach (AIContent content in update.Contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(textContent.Text);
                    Console.ResetColor();
                    break;

                case FunctionCallContent functionCall:
                    // 判断是前端还是后端工具
                    var isFrontendTool = frontendTools.Any(t =>
                        t.Name == functionCall.Name);

                    if (isFrontendTool)
                    {
                        frontendToolCalls++;
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"\n📱 [前端工具调用] {functionCall.Name}");
                    }
                    else
                    {
                        backendToolCalls++;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"\n🖥️ [后端工具调用] {functionCall.Name}");

                        // 显示后端工具参数
                        if (functionCall.Arguments != null)
                        {
                            Console.WriteLine("   📝 参数:");
                            foreach (var kvp in functionCall.Arguments)
                            {
                                Console.WriteLine($"      • {kvp.Key}: {kvp.Value}");
                            }
                        }
                    }
                    Console.ResetColor();

                    toolCallChain.Add(isFrontendTool ? $"📱{functionCall.Name}" : $"🖥️{functionCall.Name}");
                    break;

                case FunctionResultContent functionResult:
                    // 后端工具结果显示
                    if (!frontendTools.Any(t => toolCallChain.LastOrDefault()?.Contains(t.Name) ?? false))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        string resultPreview = functionResult.Result?.ToString() ?? "null";
                        if (resultPreview.Length > 150)
                        {
                            resultPreview = resultPreview.Substring(0, 150) + "...";
                        }
                        Console.WriteLine($"   ✅ 结果: {resultPreview}");
                        Console.ResetColor();
                    }
                    break;

                case ErrorContent errorContent:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ 错误: {errorContent.Message}");
                    Console.ResetColor();
                    break;
            }
        }
    }

    // 显示工具调用链
    Console.WriteLine();
    Console.WriteLine("━━━━━━━━━━━━━━ 处理完成 ━━━━━━━━━━━━━━━");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"[✅ Run Finished]");
    Console.WriteLine($"   📱 前端工具调用: {frontendToolCalls}");
    Console.WriteLine($"   🖥️ 后端工具调用: {backendToolCalls}");

    if (toolCallChain.Count > 0)
    {
        Console.WriteLine($"   🔗 调用链: {string.Join(" → ", toolCallChain)}");
    }

    Console.ResetColor();
    Console.WriteLine();
}

Console.WriteLine("👋 再见！");
Console.ReadKey();