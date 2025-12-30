namespace LoopFlow.Constants;

public static class QualityCheckConstants
{
    // 质检标准：礼貌度 ≥ 85，准确性 ≥ 90，合规性必须 100%
    // 注意：阈值设置较高，配合第一次生成简化版本，确保能体现循环改进过程
    public const int PolitenessThreshold = 85;
    public const int AccuracyThreshold = 90;
}