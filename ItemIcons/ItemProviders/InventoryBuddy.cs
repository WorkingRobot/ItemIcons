using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryBuddy : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Saddlebag;

    public override string AddonName => "InventoryBuddy";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(10, 35)
            .Concat(Enumerable.Range(46, 35))
            .Select(i => GetDragDropIcon(drawnAddon, (uint)i));

    // 0 = left
    // 1 = right
    private static int? GetSelectedSaddlebag(nint addon)
    {
        var inventory = (AtkUnitBase*)addon;
        for (var i = 0; i < 2; ++i)
        {
            var id = (uint)i + 7;
            if (NodeUtils.GetNodeById(&inventory->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentBase, 3)->GetAsAtkNineGridNode()->AtkResNode.IsVisible)
                return i;
        }
        return null;
    }

    public override IEnumerable<Item?> GetItems(nint addon) =>
            (
                (GetSelectedSaddlebag(addon) ?? 0) == 0 ?
                    ContainerType.SaddleBag :
                    ContainerType.PremiumSaddleBag
            )
            .GetContainer()
            .Select(Item.FromInventoryItem);
}
