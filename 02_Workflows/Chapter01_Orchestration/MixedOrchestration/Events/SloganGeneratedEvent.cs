using MixedOrchestration.Models;
using Microsoft.Agents.AI.Workflows;

namespace MixedOrchestration.Events;

public sealed class SloganGeneratedEvent : WorkflowEvent
{
    private readonly SloganResult _sloganResult;

    public SloganGeneratedEvent(SloganResult sloganResult) 
        : base(sloganResult)
    {
        this._sloganResult = sloganResult;
    }

    public override string ToString() =>
        $"📝 [标语生成] {_sloganResult.Slogan}";
}
