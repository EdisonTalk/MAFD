namespace SharedState.Data;

internal static class SharedStateSampleData
{
    private static readonly IReadOnlyDictionary<string, string> Documents = new Dictionary<string, string>
    {
        ["ProductBrief"] = "MAF Workflow 让 .NET 团队可以像积木一样组合 Agent、Executor 与工具, 支持流式事件、并发节点和可观测性。\n\n它强调企业级能力, 包括状态管理、依赖注入、权限控制, 适合搭建端到端 AI 业务流程。",
        ["WeeklyReport"] = "本周平台完成了 Shared State 功能的代码走查, 已经覆盖 Fan-out/Fan-in, Loop, Human-in-the-Loop 三种场景。\n\n下周计划: 1) 集成多模型投票; 2) 增加异常回滚; 3) 落地监控指标。"
    };

    public static string GetDocument(string name)
        => Documents.TryGetValue(name, out var content)
            ? content
            : throw new ArgumentException($"未找到文档: {name}");
}