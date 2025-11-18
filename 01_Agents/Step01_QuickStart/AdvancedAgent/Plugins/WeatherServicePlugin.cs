using System.ComponentModel;

namespace AdvancedAgent.Plugins;

public class WeatherServicePlugin
{
    [Description("Get current weather for a location")]
    public static async Task<string> GetWeatherAsync([Description("The location to get the weather for.")] string location)
    {
        return $"The weather in {location} is cloudy with a high of 15°C.";
    }
}