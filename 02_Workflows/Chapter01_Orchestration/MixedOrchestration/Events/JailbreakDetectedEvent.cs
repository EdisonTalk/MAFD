using Microsoft.Agents.AI.Workflows;
using MixedOrchestration.Models;

namespace MixedOrchestration.Events;

public sealed class JailbreakDetectedEvent : WorkflowEvent
{
    private readonly DetectionResult _detectionResult;

    public JailbreakDetectedEvent(DetectionResult detectionResult) : base(detectionResult)
    {
        this._detectionResult = detectionResult;
    }

    public override string ToString() =>
        $"""
        🚨 [越狱检测]
        越狱: {_detectionResult.IsJailbreak}
        输入: {_detectionResult.UserInput}
        时间: {_detectionResult.DetectTime}
        """;
}