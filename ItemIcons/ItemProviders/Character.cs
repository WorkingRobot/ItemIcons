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

    private static readonly uint[] IconOrder =
    [
        49, 50, 51, 52, 54, 53, 55, 56, 57, 58, 59, 60, 61
    ];

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        IconOrder.Select(i => GetBaseDragDropIcon(drawnAddon, i));

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var container = InventoryType.EquippedItems.GetContainer();
        
        //               Skip belt slot
        return container.Where((i, idx) => idx != 5).Select(Item.FromInventoryItem);
    }
}
