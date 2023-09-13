using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using ItemIcons.IconTypes;
using ItemIcons.ItemProviders;
using System.Collections.Generic;

namespace ItemIcons.IconProviders;

internal sealed class Obtained : SingleIconProvider
{
    public override string Name => "Obtained";

    public override BaseIcon Icon => new TextureIcon("ui/uld/RecipeNoteBook_hr1.tex", new(128, 60, 40, 40));

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
        else if (ret == 4)
        {
            unobtainableItems.Add(itemId);
            return false;
        }
        else if (ret == 1)
        {
            obtainedItems.Add(itemId);
            return true;
        }
        PluginLog.Debug($"Unknown response: {itemId} -> {ret}");
        return false;
    }
}
