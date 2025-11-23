using System.ComponentModel;

namespace AdvancedAgent.Plugins;

public class DateTimePlugin
{
    [Description("The current datetime offset.")]
    public static string GetDateTime()
    => DateTimeOffset.Now.ToString();
}