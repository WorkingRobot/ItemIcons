using FFXIVClientStructs.FFXIV.Client.Game;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class RetainerCharacter : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.RetainerCharacter;

    public override string AddonName => "RetainerCharacter";

    private static readonly uint[] IconOrder =
    [
        137, 138, 139, 140, 142, 141, 143, 144, 145, 146, 147, 148 // No soul crystal
    ];

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        IconOrder.Select(i => GetBaseDragDropIcon(drawnAddon, i));

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var container = InventoryType.RetainerEquippedItems.GetContainer();

        //               Skip belt slot
        return container.Where((i, idx) => idx != 5).Select(Item.FromInventoryItem);
    }
}
