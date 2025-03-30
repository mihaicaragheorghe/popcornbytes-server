namespace PopcornBytes.Api.Kernel;

public class CacheNode<V>
{
    public V Value { get; set; }
    public CacheNode<V>? Next { get; set; }
    public CacheNode<V>? Prev { get; set; }
    public DateTime AddedAt { get; }

    public CacheNode(V value)
    {
        Value = value;
        AddedAt = DateTime.UtcNow;
    }
}

public class LRUCache<K, V> where K : notnull
{
    private readonly int _capacity;
    private int _length;

    private CacheNode<V>? _head;
    private CacheNode<V>? _tail;

    private readonly Lock _mutex = new();
    private readonly Dictionary<K, CacheNode<V>> _lookup = new();
    private readonly Dictionary<CacheNode<V>, K> _reverseLookup = new();

    private readonly TimeSpan _expirationTime;

    public LRUCache(int capacity, TimeSpan expirationTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _capacity = capacity;
        _expirationTime = expirationTime;
    }

    public void Set(K key, V value)
    {
        using (_mutex.EnterScope())
        {
            if (_lookup.TryGetValue(key, out var node))
            {
                Detach(node);
                Prepend(node);
                node.Value = value;
            }
            else
            {
                node = new CacheNode<V>(value);
                _length++;
                Prepend(node);
                TrimCache();

                _lookup[key] = node;
                _reverseLookup[node] = key;
            }
        }
    }

    public bool TryGetValue(K key, out V value)
    {
        value = default!;
        if (!_lookup.TryGetValue(key, out var node))
        {
            return false;
        }

        using (_mutex.EnterScope())
        {
            // Lazy expiration in case a node is constantly looked up and won't be cleared by LRU
            if (node.AddedAt.Add(_expirationTime) < DateTime.UtcNow)
            {
                node = _lookup[key];
                _lookup.Remove(key);
                _reverseLookup.Remove(node);
                return false;
            }

            Detach(node);
            Prepend(node);
            value = node.Value;
            return true;
        }
    }

    private void Detach(CacheNode<V> cacheNode)
    {
        if (cacheNode.Prev != null)
        {
            cacheNode.Prev.Next = cacheNode.Next;
        }

        if (cacheNode.Next != null)
        {
            cacheNode.Next.Prev = cacheNode.Prev;
        }

        if (_head == cacheNode)
        {
            _head = _head.Next;
        }

        if (_tail == cacheNode)
        {
            _tail = _tail.Prev;
        }

        if (_length == 1)
        {
            _head = null;
            _tail = null;
        }

        cacheNode.Next = null;
        cacheNode.Prev = null;
    }

    private void Prepend(CacheNode<V> cacheNode)
    {
        if (_head == null)
        {
            _head = cacheNode;
            _tail = cacheNode;
            return;
        }

        cacheNode.Next = _head;
        _head.Prev = cacheNode;
        _head = cacheNode;
    }

    private void TrimCache()
    {
        if (_length <= _capacity) return;

        ArgumentNullException.ThrowIfNull(_tail);
        var tail = _tail;
        Detach(_tail);

        var key = _reverseLookup[tail];
        _lookup.Remove(key);
        _reverseLookup.Remove(tail);
        _length--;
    }
}