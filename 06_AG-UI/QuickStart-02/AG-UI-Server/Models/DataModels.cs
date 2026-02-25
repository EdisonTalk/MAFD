using System.Text.Json.Serialization;

namespace AgUIBackend.Models;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 📦 数据模型定义
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

internal sealed class RestaurantSearchResult
{
    public string Location { get; set; } = string.Empty;
    public double SearchRadius { get; set; }
    public int TotalResults { get; set; }
    public NearbyRestaurant[] Restaurants { get; set; } = [];
}

internal sealed class NearbyRestaurant
{
    public string Name { get; set; } = string.Empty;
    public string Cuisine { get; set; } = string.Empty;
    public double Distance { get; set; }
    public double Rating { get; set; }
    public string Address { get; set; } = string.Empty;
}

internal sealed class RestaurantDetail
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
    public string[] RecommendedDishes { get; set; } = [];
    public string Phone { get; set; } = string.Empty;
    public bool HasParking { get; set; }
    public bool AcceptsReservation { get; set; }
}

// JSON 序列化上下文
[JsonSerializable(typeof(RestaurantSearchResult))]
[JsonSerializable(typeof(NearbyRestaurant))]
[JsonSerializable(typeof(RestaurantDetail))]
internal sealed partial class MixedToolsJsonContext : JsonSerializerContext;