using Microsoft.Agents.AI;

namespace AgentSkillDemo.Skills.Caching;

/// <summary>
/// 带 TTL（Time-To-Live）缓存的 Source 装饰器。
/// 第一次调用时从内层 Source 获取技能并缓存，TTL 过期后重新获取。
/// </summary>
public sealed class CachingSkillsSource : AgentSkillsSource
{
    private readonly AgentSkillsSource _innerSource;
    private readonly TimeSpan _ttl;
    private IList<AgentSkill>? _cache;
    private DateTime _cacheExpiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public CachingSkillsSource(AgentSkillsSource innerSource, TimeSpan ttl)
    {
        _innerSource = innerSource;
        _ttl = ttl;
    }

    public override async Task<IList<AgentSkill>> GetSkillsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache != null && DateTime.UtcNow < _cacheExpiresAt)
        {
            Console.WriteLine($"⚡ [CachingSource] 命中缓存（过期时间: {_cacheExpiresAt.ToLocalTime():HH:mm:ss}）");
            return _cache;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // 双重检查锁（避免并发刷新）
            if (_cache != null && DateTime.UtcNow < _cacheExpiresAt)
                return _cache;

            Console.WriteLine("🔄 [CachingSource] 缓存未命中，从内层 Source 刷新...");
            _cache = await _innerSource.GetSkillsAsync(cancellationToken);
            _cacheExpiresAt = DateTime.UtcNow.Add(_ttl);
            Console.WriteLine($"✅ [CachingSource] 缓存已更新，{_cache.Count} 个技能，有效至 {_cacheExpiresAt.ToLocalTime():HH:mm:ss}");
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void InvalidateCache() { _cache = null; _cacheExpiresAt = DateTime.MinValue; }
    public bool IsCacheValid => _cache != null && DateTime.UtcNow < _cacheExpiresAt;
}