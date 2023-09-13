using System.Collections.Generic;

namespace ItemIcons;

internal sealed class DelayedCache<K, V> where K : notnull
{
    private Dictionary<K, V> activeFrame = new();
    private Dictionary<K, V> cachedFrame = new();

    public void Next()
    {
        (activeFrame, cachedFrame) = (cachedFrame, activeFrame);
        activeFrame.Clear();
    }

    public void Store(K key, V value) =>
        activeFrame[key] = value;

    public bool TryGet(K key, out V value) =>
        cachedFrame.TryGetValue(key, out value!);

    public bool StoreAndGet(K key, ref V value)
    {
        Store(key, value);
        if (TryGet(key, out var cachedValue))
        {
            value = cachedValue;
            return true;
        }
        return false;
    }
}
