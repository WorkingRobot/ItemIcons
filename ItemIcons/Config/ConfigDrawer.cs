using ItemIcons.Config.Drawers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ItemIcons.Config;

public abstract class ConfigDrawer
{
    public abstract bool CanDraw(Type type);

    public abstract bool Draw<T>(ref T value, ConfigAttribute config, Stack<object?> parents);

    public static ConfigDrawer ClassDefault { get; } = new ClassDrawer();

    private static ConfigDrawer[] DefaultDrawers { get; } = new ConfigDrawer[]
    {
        new BoolDrawer(),
        new ElementDrawer(),
    };

    public static ConfigDrawer? GetDrawer(Type type) =>
        DefaultDrawers.FirstOrDefault(d => d.CanDraw(type));
}

public abstract class ConfigDrawer<T> : ConfigDrawer
{
    public sealed override bool CanDraw(Type type) =>
        type == typeof(T);

    public sealed override bool Draw<K>(ref K value, ConfigAttribute config, Stack<object?> parents)
    {
        if (typeof(K) != typeof(T))
            throw new ArgumentException($"Type mismatch ({typeof(K)}); can only draw {typeof(T)}.");
        return Draw(ref Unsafe.As<K, T>(ref value), config, parents);
    }

    public abstract bool Draw(ref T value, ConfigAttribute config, Stack<object?> parents);
}
