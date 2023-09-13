using Dalamud.Configuration;
using ItemIcons.IconProviders;
using ItemIcons.ItemProviders;
using System;
using System.Collections.Generic;

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

[Serializable]
public class ItemProviderCategoryConfig
{
    public bool Enabled { get; set; } = true;
    public bool ShowOnlyOne { get; set; }

    public Dictionary<string, bool> IconProviders { get; set; } = new();
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; }

    public Dictionary<ItemProviderCategory, ItemProviderCategoryConfig> ItemProviders { get; set; } = new();

    public bool IsItemProviderEnabled(BaseItemProvider provider) =>
        !ItemProviders.TryGetValue(provider.Category, out var config) || config.Enabled;

    public bool ShouldItemProviderShowOneIcon(BaseItemProvider provider) =>
        ItemProviders.TryGetValue(provider.Category, out var config) && config.ShowOnlyOne;

    public bool CanItemProviderMatch(BaseItemProvider provider, IconProvider iconProvider)
    {
        if (ItemProviders.TryGetValue(provider.Category, out var config))
        {
            if (config.IconProviders.TryGetValue(iconProvider.GetType().FullName!, out var canMatch))
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
