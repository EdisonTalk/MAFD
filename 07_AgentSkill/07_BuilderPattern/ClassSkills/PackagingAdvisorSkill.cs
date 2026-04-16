using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AgentSkillDemo.ClassSkills;

internal sealed class PackagingAdvisorSkill : AgentClassSkill<PackagingAdvisorSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        name: "packaging-advisor",
        description: "针对不同货品类别提供包装建议并估算包装成本。");

    protected override string Instructions => """
        当用户需要包装建议时：

        1. 先读取 packaging-guidelines。
        2. 再根据品类和数量执行 estimate-packaging-cost。
        """;

    protected override JsonSerializerOptions? SerializerOptions => null;

    [AgentSkillResource("packaging-guidelines")]
    [Description("不同货品类别的包装建议速查表。")]
    public string PackagingGuidelines => """
        # 包装建议速查

        - fragile: 双层缓冲 + 木箱加固
        - electronics: 防静电袋 + 防震泡棉
        - textile: 防潮袋 + 外层编织袋
        """;

    [AgentSkillScript("estimate-packaging-cost")]
    [Description("根据品类和数量估算包装成本，返回 JSON 结果。")]
    public static string EstimatePackagingCost(string category, int quantity)
    {
        double unitCost = category.ToLowerInvariant() switch
        {
            "fragile" => 12.5,
            "electronics" => 9.8,
            "textile" => 3.2,
            _ => 6.0,
        };

        return JsonSerializer.Serialize(new
        {
            category,
            quantity,
            unitCost,
            totalCost = Math.Round(unitCost * quantity, 2)
        });
    }
}