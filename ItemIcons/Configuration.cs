using Dalamud.Configuration;
using ItemIcons.Config;
using ItemIcons.Config.Drawers;
using ItemIcons.Config.Elements;
using ItemIcons.IconProviders;
using ItemIcons.ItemProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons;

public enum ItemProviderCategory
{
    Inventory,
    ArmouryBoard,
    Character,
    Saddlebag,
    RetainerInventory,
    ItemDetail,
    MirageBox,
    MiragePlate,
    RetainerCharacter,
    RetainerVenture,
    Loot,
    // Gathering, TODO
}

internal sealed class TrueConstructor : NewConstructor<bool>
{
    public override bool New() => true;
}

[Serializable]
[ConfigClass]
public record ItemProviderCategoryConfig
{
    [Config("Enabled")]
    public bool Enabled { get; set; } = true;

    [Config("Show Only One Icon", verifier: typeof(DisableVerifier))]
    public bool ShowOnlyOne { get; set; }

    [Config("Icon Providers", drawer: typeof(ListedBaseTypeDrawer<IconProvider, bool>))]
    [ConfigValue(verifier: typeof(DisableProviderVerifier), constructor: typeof(TrueConstructor))]
    public Dictionary<string, bool> IconProviders { get; set; } = new();

    private class DisableVerifier : DefaultVerifier
    {
        public override bool IsDisabled(object? value, Stack<object?> parents) =>
            parents.FirstOrDefault(v => v is ItemProviderCategoryConfig) is ItemProviderCategoryConfig c && !c.Enabled;
    }

    private sealed class DisableProviderVerifier : DisableVerifier
    {
        public override bool IsDisabled(object? value, Stack<object?> parents)
        {
            if (base.IsDisabled(value, parents))
                return true;

            if (parents.First() is not string v)
                return false;
            if (parents.Last() is not Configuration c)
                return false;
            if (c.IconProviders.TryGetValue(v, out var val))
                return !val.Enabled;
            return false;
        }
    }
}

[Serializable]
[ConfigClass]
public record IconProviderConfig
{
    [Config("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonIgnore]
    [Config]
    public Separator _separator => new();

    [JsonIgnore]
    [Config]
    public Text _text => new($"Among us! -> {Enabled}");
}

[Serializable]
[ConfigClass(drawer: typeof(TabBarDrawer))]
public record Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    [Config("Item Providers", drawer: typeof(SelectableEnumDrawer<ItemProviderCategory, ItemProviderCategoryConfig>))]
    public Dictionary<ItemProviderCategory, ItemProviderCategoryConfig> ItemProviders { get; set; } = new();

    // Global disable for a specific icon provider. If true, the ItemProvider must also have the icon provider as true
    [Config("Icon Providers", drawer: typeof(SelectableBaseTypeDrawer<IconProvider, IconProviderConfig>))]
    public Dictionary<string, IconProviderConfig> IconProviders { get; set; } = new();

    public bool IsItemProviderEnabled(BaseItemProvider provider) =>
        !ItemProviders.TryGetValue(provider.Category, out var config) || config.Enabled;

    public bool ShouldItemProviderShowOneIcon(BaseItemProvider provider) =>
        ItemProviders.TryGetValue(provider.Category, out var config) && config.ShowOnlyOne;

    public bool CanItemProviderMatch(BaseItemProvider provider, IconProvider iconProvider)
    {
        var iconName = iconProvider.GetType().FullName!;
        if (!IconProviders.GetValueOrDefault(iconName, new()).Enabled)
            return false;

        if (ItemProviders.TryGetValue(provider.Category, out var config))
        {
            if (config.IconProviders.TryGetValue(iconName, out var canMatch))
                return canMatch;
        }
        return true;
    }

    public void Save() =>
        Service.PluginInterface.SavePluginConfig(this);
}

public static class ItemProviderExtensions
{
    public static string ToName(this ItemProviderCategory type) => type switch
    {
        ItemProviderCategory.Inventory => "Inventory",
        ItemProviderCategory.ArmouryBoard => "Armoury Board",
        ItemProviderCategory.Character => "Equipment",
        ItemProviderCategory.Saddlebag => "Saddlebag",
        ItemProviderCategory.RetainerInventory => "Retainer Inventory",
        ItemProviderCategory.ItemDetail => "Item Tooltip",
        ItemProviderCategory.MirageBox => "Glamour Dresser",
        ItemProviderCategory.MiragePlate => "Glamour Plate",
        ItemProviderCategory.RetainerCharacter => "Retainer Equipment",
        ItemProviderCategory.RetainerVenture => "Retainer Venture",
        ItemProviderCategory.Loot => "Loot",
        _ => "Unknown"
    };
}
