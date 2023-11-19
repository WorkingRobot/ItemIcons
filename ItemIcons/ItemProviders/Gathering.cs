using FFXIVClientStructs.FFXIV.Client.UI;
using ItemIcons.AtkIcons;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class Gathering : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Gathering;

    public override string AddonName => "Gathering";

    private static readonly uint[] IconOrder = new uint[]
    {
        6, 7, 8, 9, 10, 11, 12, 13
    };

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        IconOrder.Select(i => GetCheckboxIcon(drawnAddon, i));

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var gathering = (AddonGathering*)addon;
        var itemIds = new uint[] {
            gathering->GatheredItemId1,
            gathering->GatheredItemId2,
            gathering->GatheredItemId3,
            gathering->GatheredItemId4,
            gathering->GatheredItemId5,
            gathering->GatheredItemId6,
            gathering->GatheredItemId7,
            gathering->GatheredItemId8
        };

        return itemIds.Select(i => (Item?)new Item(i));
    }
}
