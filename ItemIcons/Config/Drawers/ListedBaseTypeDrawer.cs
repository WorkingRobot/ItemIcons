using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ItemIcons.Config.Drawers;

public class ListedBaseTypeDrawer<K, V> : ListedDrawer<string, V> where K : class
{
    public override IReadOnlyList<(string Key, ConfigDrawState<V> State)> Keys => KeyArray;

    private static (string Key, ConfigDrawState<V> State)[] KeyArray { get; }

    static ListedBaseTypeDrawer()
    {
        KeyArray = GetKeys().ToArray();
    }

    internal static IEnumerable<(string, ConfigDrawState<V>)> GetKeys() =>
        typeof(K).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsAssignableTo(typeof(K)))
            .Select(t => (t.FullName!, new ConfigDrawState<V>(GetName(t))));

    private static string GetName(Type t) =>
        (t.GetProperty("Name", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as string) ?? throw new ArgumentException($"Failed to get name for {t}");
}
