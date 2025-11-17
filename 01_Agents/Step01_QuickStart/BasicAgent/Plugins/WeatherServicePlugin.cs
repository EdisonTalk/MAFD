using System.ComponentModel;

namespace BasicAgent.Plugins;

public class WeatherServicePlugin
{
    [Description("Get current weather for a location")]
    public static async Task<string> GetCurrentWeatherAsync(string location)
    {
        return $"The weather in {location} is cloudy with a high of 15°C.";
    }
}