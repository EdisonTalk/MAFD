using AgUIClient.Infrastructure;
using Microsoft.Agents.AI.AGUI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

// Load Configuration
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

// Step1. Create HTTP Client
using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(60)
};

// Step2. Create AG-UI Client
var chatClient = new AGUIChatClient(httpClient, serverEndpoint);

// Step3. Create AI Agent
var agent = chatClient.AsAIAgent(
    name: "agui-client",
    description: "AG-UI Client Agent");

// Step4. Prepare for Conversation
var session = await agent.GetNewSessionAsync();
var messages = new List<ChatMessage>()
{
    new ChatMessage(ChatRole.System, "你是一个友好的AI助手，使用中文回答用户的问题。")
};

Console.WriteLine("💬 开始对话（输入 :q 或 quit 退出）\n");

while (true)
{
    Console.Write("👤 用户: ");
    string? message = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(message)) 
        continue;
    if (message is ":q" or "quit") 
        break;

    // 添加用户消息
    messages.Add(new ChatMessage(ChatRole.User, message));

    // 流式接收响应
    Console.Write("🤖 助手: ");
    await foreach (var update in agent.RunStreamingAsync(messages, session))
    {
        foreach (AIContent content in update.Contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    Console.Write(textContent.Text);
                    break;

                case UsageContent usageContent:
                    Console.WriteLine($"\n[📊 Tokens: {usageContent.Details.TotalTokenCount}]");
                    break;

                default:
                    Console.Write("Unknown content!");
                    break;
            }
        }
    }
    Console.WriteLine("\n");
}

Console.WriteLine("👋 再见！");
Console.ReadKey();