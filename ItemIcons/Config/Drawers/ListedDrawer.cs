using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ItemIcons.Config.Drawers;

public abstract class ListedDrawer<K, V> : ConfigDrawer<Dictionary<K, V>> where K : notnull
{
    public abstract IReadOnlyList<(K Key, ConfigDrawState<V> State)> Keys { get; }
    private bool IntializedKeys { get; set; }

    public override bool Draw(ref Dictionary<K, V> value, ConfigAttribute config, Stack<object?> parents)
    {
        if (!IntializedKeys)
        {
            if (config.ValueAttribute is { } attribute)
            {
                foreach (var (_, state) in Keys)
                    state.InjectValueAttribute(attribute);
            }
            IntializedKeys = true;
        }

        using var id = ImRaii.PushId(config.Name);
        using var push = parents.PushRaii(value);

        var modified = false;
        for (var i = 0; i < Keys.Count; i++)
        {
            var key = Keys[i].Key;

            if (!value.ContainsKey(key))
            {
                var val = default(V) ?? Activator.CreateInstance<V>();
                if (config.ValueAttribute?.ConstructorInstance is INewConstructor c)
                    val = c.New<V>();
                value.Add(key, val);
            }

            using var pushKey = parents.PushRaii(key);
            modified |= Keys[i].State.Draw(ref CollectionsMarshal.GetValueRefOrAddDefault(value, key, out _)!, parents);
        }
        return modified;
    }
}
