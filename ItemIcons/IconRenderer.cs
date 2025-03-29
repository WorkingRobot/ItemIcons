using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.ItemProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using ItemIcons.IconProviders;
using Dalamud.Memory;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;

namespace ItemIcons;

public sealed unsafe class IconRenderer : IDisposable
{
    private static Configuration Config => Service.Configuration;

    public BaseItemProvider[] ItemProviders { get; } =
        typeof(BaseItemProvider).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsAssignableTo(typeof(BaseItemProvider)))
            .Select(t => Activator.CreateInstance(t) as BaseItemProvider ??
                throw new ArgumentException($"Failed to create {t}"))
            .ToArray();

    public IconProvider[] IconProviders { get; } =
        typeof(IconProvider).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsAssignableTo(typeof(IconProvider)))
            .Select(t => Activator.CreateInstance(t) as IconProvider ??
                throw new ArgumentException($"Failed to create {t}"))
            .ToArray();

    private readonly Dictionary<string, AtkItemIcon[]> addons = [];

    private AtkItemIcon? DragNode { get; set; }
    private nint? DraggedNode { get; set; }
    private bool DraggedNodeDrawn { get; set; }
    private ulong Timestamp { get; set; }
    public void Draw()
    {
        Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DraggedNodeDrawn = false;
        if (DragDropS.GetIcon() is { } dragNode)
        {
            DragNode = dragNode;
            if (DragDropS.GetDraggedIcon() is { } draggedNode)
                DraggedNode = (nint)draggedNode.Node;
            else
                DraggedNode = null;
        }
        else
        {
            DragNode = null;
            DraggedNode = null;
        }

        foreach (var provider in ItemProviders)
            DrawProvider(provider);

        if (!DraggedNodeDrawn && DragNode != null)
            DragNode?.ClearIcon();
    }

    private void DrawProvider(BaseItemProvider provider)
    {
        var addon = Service.GameGui.GetAddonByName(provider.AddonName);
        if (addon == nint.Zero)
            return;
        var addonPtr = (AtkUnitBase*)addon;
        if (addonPtr->RootNode == null)
            return;
        if (!addonPtr->RootNode->IsVisible())
            return;

        if (!Config.IsItemProviderEnabled(provider))
        {
            foreach (var n in provider.GetDrawnAddonNames(addon))
            {
                if (addons.TryGetValue(n, out var icons))
                {
                    foreach (var icon in icons)
                        icon.Destroy();
                    addons.Remove(n);
                }
            }
            return;
        }

        var iconProviders = IconProviders.Where(i => Config.CanItemProviderMatch(provider, i)).ToArray();
        var onlyOneIcon = Config.ShouldItemProviderShowOneIcon(provider);

        foreach (var (icon, itemNullable) in provider.GetDrawnAddonNames(addon)
            .Select(n => (Name: n, Addon: Service.GameGui.GetAddonByName(n)))
            .Where(a => a.Addon != nint.Zero)
            .SelectMany(a => GetIcons(provider, a.Name, a.Addon))
            .Zip(provider.GetItems(addon)))
        {
            if (itemNullable is not { } item)
            {
                icon.ClearIcon();
                continue;
            }

            // No item in slot
            if (item.ItemId == 0)
            {
                icon.ClearIcon();
                continue;
            }

            if (!DraggedNodeDrawn && (nint)icon.Node == DraggedNode)
            {
                RenderIcon(DragNode!, item, iconProviders, onlyOneIcon);
                DraggedNodeDrawn = true;
            }

            // Icon isn't visible (could be dragged around)
            if (!icon.Node->IsVisible())
                continue;

            RenderIcon(icon, item, iconProviders, onlyOneIcon);
        }
    }

    private AtkItemIcon[] GetIcons(BaseItemProvider provider, string addonName, nint addon)
    {
        if (!addons.TryGetValue(addonName, out var icons))
        {
            var providedIcons = GetIconsUncached(provider, addonName, addon);
            if (providedIcons == null)
                return [];
            addons[addonName] = icons = providedIcons;
        }
        return icons;
    }

    private static AtkItemIcon[]? GetIconsUncached(BaseItemProvider provider, string addonName, nint addon)
    {
        IEnumerable<AtkItemIcon> iconEnum;
        try
        {
            iconEnum = provider.GetIcons(addon);
        }
        catch (ArgumentOutOfRangeException e)
        {
            Log.Error(e, $"Could not get icons for provider {provider.GetType().Name} for addon {addonName}.");
            return null;
        }
        return iconEnum.ToArray();
    }

    private const int MaxIconCarouselSize = 4;
    private void RenderIcon(AtkItemIcon icon, Item item, IEnumerable<IconProvider> iconProviders, bool onlyOneIcon)
    {
        Span<uint> icons = stackalloc uint[MaxIconCarouselSize];
        var iconCount = 0;
        foreach (var iconProvider in iconProviders)
        {
            var match = iconProvider.GetMatch(item);
            if (match.HasValue)
            {
                icons[iconCount++] = match.Value;
                if (iconCount == MaxIconCarouselSize || onlyOneIcon)
                    break;
            }
        }
        if (iconCount == 0)
            icon.ClearIcon();
        else
            icon.SetIcons(icons[..iconCount], Timestamp);
    }

    public void Dispose()
    {
        foreach (var icon in ItemProviders)
            icon?.Dispose();

        foreach (var cachedIcon in addons.Values.SelectMany(a => a))
            cachedIcon?.Destroy();
    }

    public void SetupAddon(AtkUnitBase* addon)
    {
        var name = addon->NameString;
        foreach (var provider in ItemProviders)
        {
            if (provider.GetDrawnAddonNames(nint.Zero)
                    .Any(n => name.Equals(n, StringComparison.Ordinal)))
            {
                provider.SetupAddon((nint)addon);

                var providedIcons = GetIconsUncached(provider, name, (nint)addon);
                if (providedIcons != null)
                    addons[name] = providedIcons;
            }
        }
    }

    public void InvalidateAddonCache(string name)
    {
        addons.Remove(name);
    }

    public void FinalizeAddon(AtkUnitBase* addon)
    {
        var name = addon->NameString;
        foreach (var provider in ItemProviders)
        {
            if (provider.GetDrawnAddonNames(nint.Zero)
                    .Any(n => name.Equals(n, StringComparison.Ordinal)))
            {
                provider.FinalizeAddon((nint)addon);
                InvalidateAddonCache(name);
            }
        }
    }
}
