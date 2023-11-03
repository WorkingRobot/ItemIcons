using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ItemIcons.Config.Drawers;

internal sealed class ClassDrawer : ConfigDrawer
{
    private Dictionary<Type, (PropertyInfo, ConfigAttribute)[]> Attributes { get; } = new();

    public override bool CanDraw(Type type) =>
        true;

    public override bool Draw<T>(ref T value, ConfigAttribute config, Stack<object?> parents)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        using var id = ImRaii.PushId(config.Name);
        using var push = parents.PushRaii(value);

        if (!Attributes.TryGetValue(typeof(T), out var attribs))
        {
            Attributes[typeof(T)] = attribs =
                typeof(T).GetProperties()
                .Select(prop => (prop, prop.GetCustomAttribute<ConfigAttribute>()))
                .Where(prop => prop.Item2 is not null)
                .Select(prop => (prop.prop, prop.Item2!)).ToArray();

            foreach(var attrib in attribs)
                attrib.Item2.InitializeProperty(attrib.Item1);
        }

        var modified = false;
        foreach (var (info, subConfig) in attribs)
            modified |= subConfig.DrawProperty(ref value, info, parents);

        return modified;
    }
}
