using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryBuddy : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Saddlebag;

    public override string AddonName => "InventoryBuddy";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        for (uint i = 10; i < 45; ++i)
            yield return GetDragDropIcon(drawnAddon, i);
        for (uint i = 46; i < 81; ++i)
            yield return GetDragDropIcon(drawnAddon, i);
    }

    public override IEnumerable<Item?> GetItems(nint addon) =>
        ContainerType.SaddleBag.GetContainer().Select(Item.FromInventoryItem);
}
