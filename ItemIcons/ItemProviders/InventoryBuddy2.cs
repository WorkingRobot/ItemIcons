using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryBuddy2 : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Saddlebag;

    public override string AddonName => "InventoryBuddy2";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        for (uint i = 10; i < 45; ++i)
            yield return GetDragDropIcon(drawnAddon, i);
        for (uint i = 46; i < 81; ++i)
            yield return GetDragDropIcon(drawnAddon, i);
    }

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
