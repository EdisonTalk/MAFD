namespace ParallelExecution.Models;

internal class PriceQueryDto
{
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string TargetRegion { get; private set; }

    public PriceQueryDto(string productId, string productName, string targetRegion)
    {
        ProductId = productId;
        ProductName = productName;
        TargetRegion = targetRegion;
    }
}
