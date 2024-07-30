using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using ItemIcons.IconTypes;
using ItemIcons.Utils;
using System;
using System.Collections.Generic;

namespace ItemIcons.IconProviders;

internal sealed class Obtained : SingleIconProvider
{
    public override string Name => "Obtained";

    public override string Description => "Shows an icon on items that are already obtained.";

    public override BaseIcon Icon => new TextureIcon("ui/uld/RecipeNoteBook.tex", new(64, 30, 20, 20));

    private readonly HashSet<uint> unobtainableItems = [];
    private readonly HashSet<uint> obtainedItems = [];
    private readonly Dictionary<uint, DateTimeOffset> unobtainedItems = [];

    private static readonly TimeSpan UnobtainedExpirationTime = TimeSpan.FromSeconds(15);

    public override unsafe bool Matches(Item item) =>
        IsItemObtained(item.ItemId);

    private static unsafe long? IsItemActionUnlocked(uint itemId)
    {
        var itemExd = ExdModule.GetItemRowById(itemId);
        if (itemExd == null)
            return null;
        return UIState.Instance()->IsItemActionUnlocked(itemExd);
    }

    private bool IsItemObtained(uint itemId)
    {
        if (unobtainableItems.Contains(itemId))
            return false;
        if (obtainedItems.Contains(itemId))
            return true;
        if (unobtainedItems.TryGetValue(itemId, out var time))
        {
            if (DateTimeOffset.UtcNow - time < UnobtainedExpirationTime)
                return false;
        }
        var ret = IsItemActionUnlocked(itemId);
        if (!ret.HasValue)
            return false;
        // Already obtained items
        else if (ret == 1)
        {
            obtainedItems.Add(itemId);
            return true;
        }
        // Unobtained items
        else if (ret == 2)
        {
            unobtainedItems[itemId] = DateTimeOffset.UtcNow;
            return false;
        }
        // Unobtainable items
        else if (ret == 4)
        {
            unobtainableItems.Add(itemId);
            return false;
        }
        Log.Debug($"Unknown response: {itemId} -> {ret}");
        return false;
    }
}
