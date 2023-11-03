using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ItemIcons.Config.Drawers;

public sealed class TabBarDrawer : ConfigDrawer
{
    private (PropertyInfo, ConfigAttribute)[]? Attributes { get; set; }
    private Type? UsedType { get; set; }

    public override bool CanDraw(Type type) =>
        true;

    public override bool Draw<T>(ref T value, ConfigAttribute config, Stack<object?> parents)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        using var id = ImRaii.PushId(config.Name);
        using var push = parents.PushRaii(value);
        using var tabs = ImRaii.TabBar("tabs");
        if (!tabs)
            return false;

        if (UsedType is null || Attributes is null)
        {
            UsedType = typeof(T);
            Attributes = UsedType
                .GetProperties()
                .Select(prop => (prop, prop.GetCustomAttribute<ConfigAttribute>()))
                .Where(prop => prop.Item2 is not null)
                .Select(prop => (prop.prop, prop.Item2!))
                .ToArray();

            foreach (var attrib in Attributes)
                attrib.Item2.InitializeProperty(attrib.Item1);
        }
        else if (UsedType != typeof(T))
            throw new ArgumentException($"Type mismatch with {value.GetType()}. TabBarDrawer is already being used for {UsedType}.");

        var modified = false;
        foreach (var (info, subConfig) in Attributes!)
        {
            using var tab = ImRaii.TabItem(subConfig.Name);
            if (!tab)
                continue;

            modified |= subConfig.DrawProperty(ref value, info, parents);
        }
        return modified;
    }
}

