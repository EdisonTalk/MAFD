namespace AgentSkillDemo.Models;

// 模拟 API 响应中的技能定义格式
public sealed record SkillApiEntry(string Name, string Description, string Instructions, string[]? Tags = null);
