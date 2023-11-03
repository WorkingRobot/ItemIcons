using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.Config.Drawers;

public class SelectableBaseTypeDrawer<K, V> : SelectableDrawer<string, V> where K : class
{
    public override IReadOnlyList<(string Name, string Key, ConfigDrawState<V> State)> Keys => KeyArray;

    private static (string Name, string Key, ConfigDrawState<V> State)[] KeyArray { get; }

    static SelectableBaseTypeDrawer()
    {
        KeyArray = ListedBaseTypeDrawer<K, V>.GetKeys().Select(k => (k.Item2.Name, k.Item1, k.Item2)).ToArray();
    }
}
