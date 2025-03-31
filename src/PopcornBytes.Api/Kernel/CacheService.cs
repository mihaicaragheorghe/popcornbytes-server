namespace PopcornBytes.Api.Kernel;

public class CacheService<K, V> : ICacheService<K, V> where  K : notnull
{
    private readonly LRUCache<K, V> _cache;

    public CacheService(int capacity, int expirationInHours)
    {
        _cache = new LRUCache<K, V>(capacity, TimeSpan.FromHours(expirationInHours));
    }
    
    public bool TryGetValue(K key, out V value) => _cache.TryGetValue(key, out value);

    public void Set(K key, V value) => _cache.Set(key, value);
}

public interface ICacheService<in K, V> where  K : notnull
{
    bool TryGetValue(K key, out V value);

    void Set(K key, V value);
}
