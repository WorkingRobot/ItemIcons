using ItemIcons.IconTypes;
using ItemIcons.ItemProviders;
using System;
using System.Collections.Generic;

namespace ItemIcons.IconProviders;

public abstract unsafe class IconProvider
{
    public abstract string Name { get; }

    public abstract uint? GetMatch(Item item);

    private static readonly uint IdOffset = (uint)Random.Shared.Next(1000000);
    private static readonly List<BaseIcon> IdRegistry = new();
    protected static uint RegisterIcons(IEnumerable<BaseIcon> icons)
    {
        var ret = (uint)IdRegistry.Count + IdOffset;
        IdRegistry.AddRange(icons);
        var ret2 = ret;
        foreach (var icon in icons)
        {
            if (icon != null)
                icon.IconId = ret2++;
        }
        return ret;
    }

    public static BaseIcon? GetIcon(uint id)
    {
        var idx = id - IdOffset;
        if (idx >= IdRegistry.Count)
            return null;
        return IdRegistry[(int)idx];
    }

    public static void DisposeRegistry()
    {
        foreach(var icon in IdRegistry)
            icon?.Dispose();
        IdRegistry.Clear();
    }
}
