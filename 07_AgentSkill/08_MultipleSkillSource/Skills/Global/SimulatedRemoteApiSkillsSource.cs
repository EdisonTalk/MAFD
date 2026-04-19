using AgentSkillDemo.Models;
using Microsoft.Agents.AI;

namespace AgentSkillDemo.Skills.Global;

/// <summary>
/// 从远程 API 拉取技能定义的自定义 Source。
/// 实际项目中，GetSkillsFromApiAsync 会调用 HttpClient 访问技能注册中心。
/// </summary>
public sealed class SimulatedRemoteApiSkillsSource : AgentSkillsSource
{
    private readonly string _apiEndpoint;

    public SimulatedRemoteApiSkillsSource(string apiEndpoint)
    {
        _apiEndpoint = apiEndpoint;
    }

    public override async Task<IList<AgentSkill>> GetSkillsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📡 [RemoteApiSource] 从 {_apiEndpoint} 拉取技能列表...");

        await Task.Delay(500, cancellationToken); // 模拟网络延迟
        var entries = GetMockGlobalSkills();

        var skills = new List<AgentSkill>();
        foreach (var entry in entries)
        {
            try
            {
                var skill = new AgentInlineSkill(entry.Name, entry.Description, entry.Instructions);
                skills.Add(skill);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"⚠️ [RemoteApiSource] 跳过非法技能 '{entry.Name}': {ex.Message[..Math.Min(60, ex.Message.Length)]}");
            }
        }

        Console.WriteLine($"✅ [RemoteApiSource] 已成功加载 {skills.Count} 个远程技能");

        return skills;
    }

    private static IList<SkillApiEntry> GetMockGlobalSkills()
    {
        return new List<SkillApiEntry>
        {
            new("expense-report",  "（全局v1）企业费用报销政策",    "全局版报销规则：..."),
            new("hr-onboarding",   "（全局）新员工入职流程",         "入职材料清单：..."),
            new("leave-policy",    "（全局）请假制度和申请流程",      "年假/病假/事假规则：..."),
            new("manager-review",  "（全局）绩效评估指南",           "季度评估流程：..."),
            new("hr-admin-audit",  "（全局）HR 审计和合规",          "合规审查清单：..."),
        };
    }
}