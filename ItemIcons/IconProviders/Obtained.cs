using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using ItemIcons.IconTypes;
using ItemIcons.Utils;
using System.Collections.Generic;

namespace ItemIcons.IconProviders;

internal sealed class Obtained : SingleIconProvider
{
    public static string Name => "Obtained";

    public override BaseIcon Icon => new TextureIcon("ui/uld/RecipeNoteBook.tex", new(64, 30, 20, 20));

    private readonly HashSet<uint> unobtainableItems = new();
    private readonly HashSet<uint> obtainedItems = new();

    public override unsafe bool Matches(Item item) =>
        IsItemObtained(item.ItemId);

    private static unsafe long? IsItemActionUnlocked(uint itemId)
    {
        var itemExd = ExdModule.GetItemRowById(itemId);
        if (itemExd == null)
            return null;
        return UIState.Instance()->IsItemActionUnlocked(itemExd);
    }

    private unsafe bool IsItemObtained(uint itemId)
    {
        if (unobtainableItems.Contains(itemId))
            return false;
        if (obtainedItems.Contains(itemId))
            return true;
        var ret = IsItemActionUnlocked(itemId);
        if (!ret.HasValue)
            return false;
        // Unobtainable items
        else if (ret == 4)
        {
            unobtainableItems.Add(itemId);
            return false;
        }
        // Already obtained items
        else if (ret == 1)
        {
            obtainedItems.Add(itemId);
            return true;
        }
        // Unobtained items
        else if (ret == 2)
            return false;
        Log.Debug($"Unknown response: {itemId} -> {ret}");
        return false;
    }
}
