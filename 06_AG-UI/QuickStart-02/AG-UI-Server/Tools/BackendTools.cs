using AgUIBackend.Models;
using System.ComponentModel;

namespace AgUIBackend.Tools;

internal class BackendTools
{
    // 🔧 餐厅搜索工具（后端执行）
    [Description("根据位置搜索附近的餐厅。Search for nearby restaurants based on location.")]
    public static RestaurantSearchResult SearchNearbyRestaurants(
        [Description("用户当前位置描述")] string location,
        [Description("搜索半径（公里）")] double radiusKm = 2.0,
        [Description("菜系偏好（可选）")] string? cuisinePreference = null)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n🔍 [后端工具] SearchNearbyRestaurants");
        Console.WriteLine($"   📍 位置: {location}");
        Console.WriteLine($"   📏 半径: {radiusKm} km");
        Console.WriteLine($"   🍽️ 菜系偏好: {cuisinePreference ?? "不限"}");
        Console.ResetColor();

        // 模拟数据库查询
        var restaurants = new List<NearbyRestaurant>
    {
        new() { Name = "老北京炸酱面", Cuisine = "北京菜", Distance = 0.5, Rating = 4.7, Address = $"{location}东路100号" },
        new() { Name = "川香阁", Cuisine = "川菜", Distance = 0.8, Rating = 4.5, Address = $"{location}西路200号" },
        new() { Name = "粤味轩", Cuisine = "粤菜", Distance = 1.2, Rating = 4.8, Address = $"{location}南路300号" },
        new() { Name = "日式料理屋", Cuisine = "日本料理", Distance = 1.5, Rating = 4.6, Address = $"{location}北路400号" },
        new() { Name = "烤匠", Cuisine = "川菜", Distance = 2.5, Rating = 4.8, Address = $"{location}天府四街银泰城5楼" }
    };

        // 如果有菜系偏好，过滤结果
        if (!string.IsNullOrEmpty(cuisinePreference))
        {
            restaurants = restaurants
                .Where(r => r.Cuisine.Contains(cuisinePreference, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"   ✅ 找到 {restaurants.Count} 家餐厅");
        Console.ResetColor();

        return new RestaurantSearchResult
        {
            Location = location,
            SearchRadius = radiusKm,
            TotalResults = restaurants.Count,
            Restaurants = restaurants.ToArray()
        };
    }

    // 🔧 获取餐厅详情（后端执行）
    [Description("获取指定餐厅的详细信息，包括营业时间、菜单推荐等。Get detailed information about a specific restaurant.")]
    public static RestaurantDetail GetRestaurantDetail(
        [Description("餐厅名称")] string restaurantName)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n📖 [后端工具] GetRestaurantDetail");
        Console.WriteLine($"   🏪 餐厅: {restaurantName}");
        Console.ResetColor();

        // 模拟数据库查询
        var detail = new RestaurantDetail
        {
            Name = restaurantName,
            Description = $"{restaurantName}是一家知名的特色餐厅，拥有20年历史。",
            OpeningHours = "10:00 - 22:00",
            PriceRange = "人均 80-150 元",
            RecommendedDishes = ["招牌菜", "特色小吃", "主厨推荐"],
            Phone = "010-12345678",
            HasParking = true,
            AcceptsReservation = true
        };

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"   ✅ 获取成功");
        Console.ResetColor();

        return detail;
    }
}