using FFXIVClientStructs.FFXIV.Client.UI;
using ItemIcons.AtkIcons;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class Gathering : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Gathering;

    public override string AddonName => "Gathering";

    private static readonly uint[] IconOrder =
    [
        17, 18, 19, 20, 21, 22, 23, 24
    ];

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        IconOrder.Select(i => GetCheckboxIcon(drawnAddon, i));

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var gathering = (AddonGathering*)addon;
        var itemIds = gathering->ItemIds.ToArray();

        return itemIds.Select(i => (Item?)new Item(i));
    }
}
