using A2A;

namespace WeatherAgentServer;

public class WeatherAgent
{
    public void Attach(ITaskManager taskManager)
    {
        taskManager.OnMessageReceived = QueryWeatherAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    private Task<A2AResponse> QueryWeatherAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
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
                    🌤️ **天气查询结果**

                    查询时间：{DateTime.Now:yyyy-MM-dd HH:mm}

                    **北京天气**
                    - 今日：晴转多云，气温 -2°C ~ 8°C
                    - 明日：多云，气温 0°C ~ 10°C
                    - 后日：阴，气温 2°C ~ 9°C

                    **上海天气**
                    - 今日：多云，气温 5°C ~ 12°C
                    - 明日：小雨，气温 6°C ~ 10°C
                    - 后日：阴转晴，气温 4°C ~ 11°C

                    👔 穿衣建议：北京较冷，建议穿羽绒服；上海温和，建议穿夹克外套，带好雨具。
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
            Name = "weather agent",
            Description = "weather information agent",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [
                new AgentSkill
                {
                    Id = "weather-query",
                    Name = "天气查询",
                    Description = "查询指定城市的天气预报，包括温度、降水概率、穿衣建议等",
                    Tags = ["weather", "forecast", "climate"],
                    Examples = ["上海明天天气怎么样", "成都这周的天气预报", "杭州下雨吗"],
                    InputModes = ["text"],
                    OutputModes = ["text"]
                }
                ],
        });
    }
}