using A2A;

namespace PlanAgentServer;

public class PlanAgent
{
    public void Attach(ITaskManager taskManager)
    {
        taskManager.OnMessageReceived = QueryPlansAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    private Task<A2AResponse> QueryPlansAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<A2AResponse>(cancellationToken);
        }

        // Process the message
        var messageText = messageSendParams.Message.Parts.OfType<TextPart>().First().Text;

        // Create and return an artifact
        var message = new AgentMessage()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = [new TextPart() {
                Text = $"""
                    🎡 **景点推荐**

                    为您推荐上海必游景点：

                    **历史文化类**
                    1. 🏛️ 外滩 - 欣赏万国建筑博览群
                    2. 🏯 豫园 - 江南古典园林代表
                    3. 🕌 城隍庙 - 品尝地道上海小吃

                    **现代都市类**
                    4. 🗼 东方明珠塔 - 上海地标，俯瞰浦江两岸
                    5. 🌆 陆家嘴 - 金融中心，上海之巅
                    6. 🛍️ 南京路步行街 - 购物天堂

                    **文艺休闲类**
                    7. 🎨 田子坊 - 文艺小店聚集地
                    8. 📚 武康路 - 梧桐树下的法式风情
                    9. 🌳 世纪公园 - 城市绿肺，亲子游首选

                    📅 建议游玩时间：3-4 天可覆盖主要景点
                    """
            }]
        };

        return Task.FromResult<A2AResponse>(message);
    }

    private Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities()
        {
            Streaming = true,
            PushNotifications = false,
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "plan agent",
            Description = "travel plan & attraction agent",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [
                new AgentSkill
                {
                    Id = "attraction-recommendation",
                    Name = "景点推荐",
                    Description = "推荐目的地的热门景点和游玩路线，包括历史文化、现代都市、文艺休闲等类型",
                    Tags = ["attraction", "sightseeing", "tourism", "travel"],
                    Examples = ["上海有什么好玩的", "北京必去的景点", "杭州西湖怎么玩"],
                    InputModes = ["text"],
                    OutputModes = ["text"]
                }
                ],
        });
    }
}