using System;

namespace ItemIcons.Config.Drawers;

public interface INewConstructor
{
    public T New<T>();
}

public sealed class DefaultConstructor : INewConstructor
{
    public T New<T>() => Activator.CreateInstance<T>();
}

public abstract class NewConstructor<T> : INewConstructor
{
    public K New<K>()
    {
        if (typeof(T) == typeof(K))
            return (K)(object?)New()!;
        throw new ArgumentException($"Cannot create {typeof(K)}. Only {typeof(T)} is supported.");
    }

    public abstract T New();
}
