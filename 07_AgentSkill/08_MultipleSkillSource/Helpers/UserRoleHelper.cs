using AgentSkillDemo.Models;
using Microsoft.Agents.AI;

namespace AgentSkillDemo.Helpers;

public static class UserRoleHelper
{
    public static bool IsSkillVisibleTo(AgentSkill skill, EmployeeRole role)
    {
        var name = skill.Frontmatter.Name;

        // 管理技能仅经理和HR可见
        if (name.StartsWith("manager-") && role == EmployeeRole.Employee) 
            return false;
        // HR管理技能仅HR可见
        if (name.StartsWith("hr-admin-") && role != EmployeeRole.HRAdmin) 
            return false;

        return true;
    }
}
