namespace CommonShared;

public sealed class OpenAIProvider
{
    public string ApiKey { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
}