using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryRetainerLarge : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.RetainerInventory;

    public override string AddonName => "InventoryRetainerLarge";

    public override IEnumerable<string> GetDrawnAddonNames(nint addon)
    {
        if (addon == nint.Zero)
            return ["RetainerGrid0", "RetainerGrid1", "RetainerGrid2", "RetainerGrid3", "RetainerGrid4"];

        var (pageA, pageB) = GetSelectedInventoryTypes(addon);
        var ret = new List<string>(2);
        if (pageA.HasValue)
            ret.Add($"RetainerGrid{pageA}");
        if (pageB.HasValue)
            ret.Add($"RetainerGrid{pageB}");
        return ret;
    }

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(3, 35)
            .Select(i =>
                TryGetDragDropIcon(drawnAddon, (uint)i) ??
                    throw new ArgumentOutOfRangeException(nameof(drawnAddon), drawnAddon, "Could not get retainer's item icons. Try again next frame.")
            );

    private static (int? PageA, int? PageB) GetSelectedInventoryTypes(nint addon)
    {
        var board = (AtkUnitBase*)addon;
        for (var i = 0; i < 3; ++i)
        {
            var id = (uint)i + 3;
            if (NodeUtils.GetNodeById(&board->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentButton.AtkComponentBase, 3)->GetAsAtkNineGridNode()->AtkResNode.IsVisible())
                return (i * 2, i != 2 ? (i * 2) + 1 : null);
        }
        return (null, null);
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var (pageA, pageB) = GetSelectedInventoryTypes(addon);

        var aContainer = pageA.HasValue ? ContainerType.Retainer.GetContainerPage(pageA.Value, 35) : Enumerable.Empty<InventoryItem?>();
        var bContainer = pageB.HasValue ? ContainerType.Retainer.GetContainerPage(pageB.Value, 35) : Enumerable.Empty<InventoryItem?>();
        return aContainer.Concat(bContainer).Select(Item.FromInventoryItem);
    }
}
