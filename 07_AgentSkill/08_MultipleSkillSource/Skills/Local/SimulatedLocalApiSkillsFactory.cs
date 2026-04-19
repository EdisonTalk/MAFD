using Microsoft.Agents.AI;

namespace AgentSkillDemo.Skills.Local;

internal class SimulatedLocalApiSkillsFactory
{
    public static async Task<IList<AgentInlineSkill>> GetSkillsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🏠 [LocalApiFactory] 正在从本地获取技能列表...");
        
        var localSkills = new List<AgentInlineSkill>
        {
            // 覆盖全局 expense-report，使用 Contoso 定制规则
            new AgentInlineSkill("expense-report", "（Contoso定制v2）企业费用报销政策",
                """
                # Contoso 定制报销规则（2025版）
                - 差旅费上限提升至 8000 元/次
                - 新增"远程协作设备补贴"类目，≤3000元免审批
                - 年末报销截止日期：12月25日
                """),
            // 新增：Contoso 特有的技能
            new AgentInlineSkill("contoso-benefits", "Contoso 员工福利计划详情", "福利：弹性办公、年度体检、学习补贴..."),
        };

        Console.WriteLine($"✅ [LocalApiFactory] 已成功加载 {localSkills.Count} 个本地技能");

        return localSkills;
    }
}
