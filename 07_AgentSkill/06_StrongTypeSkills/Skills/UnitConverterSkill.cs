using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AgentSkillDemo.Skills;

internal sealed class UnitConverterSkill : AgentClassSkill<UnitConverterSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "unit-converter",
        "Convert between common units using multiplication factors.");

    protected override string Instructions => """
        当用户询问距离或重量换算时：

        1. 先读取 conversion-table 资源，找到对应换算系数。
        2. 再调用 convert 脚本执行计算，参数为用户输入的数值value和换算系数factor。
        3. 回复内容需要清晰地展示换算系数、换算过程和换算结果，并同时标明换算前后的两个单位。
        """;

    protected override JsonSerializerOptions? SerializerOptions => null;

    [AgentSkillResource("conversion-table")]
    [Description("常见距离与重量换算系数表。")]
    public string ConversionTable => """
        # Conversion Table

        Formula: result = value × factor

        | From       | To         | Factor   |
        |------------|------------|----------|
        | miles      | kilometers | 1.60934  |
        | kilometers | miles      | 0.621371 |
        | pounds     | kilograms  | 0.453592 |
        | kilograms  | pounds     | 2.20462  |
        """;

    [AgentSkillScript("convert")]
    [Description("按 value × factor 执行换算，并返回 JSON。")]
    public static string ConvertUnits(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}