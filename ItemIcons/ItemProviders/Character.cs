using FFXIVClientStructs.FFXIV.Client.Game;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class Character : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Character;

    public override string AddonName => "Character";

    private static readonly uint[] IconOrder = new uint[]
    {
        48, 49, 50, 51, 53, 52, 54, 55, 56, 57, 58, 59, 60
    };

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        foreach(var i in IconOrder)
            yield return GetBaseDragDropIcon(drawnAddon, i);
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var container = InventoryType.EquippedItems.GetContainer();
        
        //               Skip belt slot
        return container.Where((i, idx) => idx != 5).Select(Item.FromInventoryItem);
    }
}
