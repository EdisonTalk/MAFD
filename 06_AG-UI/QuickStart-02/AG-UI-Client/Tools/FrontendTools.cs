using System.ComponentModel;

namespace AgUIFrontend.Tools;

internal class FrontendTools
{
    // 🔧 获取用户位置（前端工具 - 只有客户端能访问 GPS）
    [Description("获取用户当前的地理位置信息。这是客户端设备功能，用于获取 GPS 位置。Get the user's current location from GPS.")]
    public static string GetUserLocation()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n📍 [前端工具] GetUserLocation");
        Console.WriteLine("   🔄 正在访问设备 GPS...");
        Console.ResetColor();

        // 模拟 GPS 获取延迟
        Thread.Sleep(800);

        // 模拟不同的位置（随机选择）
        string[] locations =
        [
            "北京市朝阳区三里屯",
            "上海市浦东新区陆家嘴",
            "广州市天河区珠江新城",
            "深圳市南山区科技园",
            "成都市高新区天府软件园"
        ];

        Random random = new();
        string location = locations[random.Next(locations.Length)];

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"   ✅ GPS 定位成功: {location}");
        Console.ResetColor();

        return location;
    }

    // 🔧 获取用户偏好设置（前端工具 - 访问本地存储）
    [Description("获取用户保存的餐饮偏好设置。Get user's saved dining preferences.")]
    public static string GetUserPreferences()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n⚙️ [前端工具] GetUserPreferences");
        Console.WriteLine("   🔄 读取本地偏好设置...");
        Console.ResetColor();

        // 模拟读取本地存储
        Thread.Sleep(300);

        string preferences = "偏好菜系: 川菜、粤菜; 价位: 中等; 忌口: 无";

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"   ✅ 读取成功: {preferences}");
        Console.ResetColor();

        return preferences;
    }
}