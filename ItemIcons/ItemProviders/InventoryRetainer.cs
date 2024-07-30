using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryRetainer : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.RetainerInventory;

    public override string AddonName => "InventoryRetainer";

    public override IEnumerable<string> GetDrawnAddonNames(nint addon) => ["RetainerGrid"];

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(3, 35)
            .Select(i =>
                TryGetDragDropIcon(drawnAddon, (uint)i) ??
                    throw new ArgumentOutOfRangeException(nameof(drawnAddon), drawnAddon, "Could not get retainer's item icons. Try again next frame.")
            );

    private static int? GetSelectedInventoryPage(nint addon)
    {
        var board = (AtkUnitBase*)addon;
        for (var i = 0; i < 5; ++i)
        {
            var id = (uint)i + 3;
            if (NodeUtils.GetNodeById(&board->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentButton.AtkComponentBase, 3)->GetAsAtkImageNode()->AtkResNode.IsVisible())
                return i;
        }
        return null;
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var page = GetSelectedInventoryPage(addon);
        return page != null ?
            ContainerType.Retainer.GetContainerPage(page.Value, 35).Select(Item.FromInventoryItem) :
            [];
    }
}
