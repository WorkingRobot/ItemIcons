using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryLarge : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Inventory;

    public override string AddonName => "InventoryLarge";

    public override IEnumerable<string> GetDrawnAddonNames(nint addon) =>
        new[] { "InventoryGrid0", "InventoryGrid1" };

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        for (uint i = 3; i < 38; ++i)
            yield return GetDragDropIcon(drawnAddon, i);
    }

    private static (int? PageA, int? PageB) GetSelectedInventoryTypes(nint addon)
    {
        var board = (AtkUnitBase*)addon;
        for (var i = 0; i < 2; ++i)
        {
            var id = (uint)i + 7;
            if (NodeUtils.GetNodeById(&board->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentBase, 3)->GetAsAtkNineGridNode()->AtkResNode.IsVisible)
                return (i * 2, (i * 2) + 1);
        }
        return (null, null);
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var (pageA, pageB) = GetSelectedInventoryTypes(addon);

        var aContainer = pageA.HasValue ? ContainerType.Inventory.GetContainerPage(pageA.Value) : Enumerable.Empty<InventoryItem?>();
        var bContainer = pageB.HasValue ? ContainerType.Inventory.GetContainerPage(pageB.Value) : Enumerable.Empty<InventoryItem?>();
        return aContainer.Concat(bContainer).Select(Item.FromInventoryItem);
    }
}
