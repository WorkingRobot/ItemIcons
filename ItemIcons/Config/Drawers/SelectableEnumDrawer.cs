using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.Config.Drawers;

public class SelectableEnumDrawer<K, V> : SelectableDrawer<K, V> where K : struct, Enum
{
    public override IReadOnlyList<(string Name, K Key, ConfigDrawState<V> State)> Keys => KeyArray;

    private static (string Name, K Key, ConfigDrawState<V> State)[] KeyArray { get; }

    static SelectableEnumDrawer()
    {
        KeyArray = ListedEnumDrawer<K, V>.GetKeys().Select(k => (k.Item2.Name, k.Item1, k.Item2)).ToArray();
    }
}
