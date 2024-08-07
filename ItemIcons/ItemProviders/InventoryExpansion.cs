using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class InventoryExpansion : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Inventory;

    public override string AddonName => "InventoryExpansion";

    public override IEnumerable<string> GetDrawnAddonNames(nint addon) =>
        ["InventoryGrid0E", "InventoryGrid1E", "InventoryGrid2E", "InventoryGrid3E"];

    private const uint ItemsNodeId = 7;
    private const uint KeyItemsAndCrystalsNodeId = 8;

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(3, 35).Select(i => GetDragDropIcon(drawnAddon, (uint)i));

    private static bool IsTabSelected(nint addon, uint nodeId)
    {
        var board = (AtkUnitBase*)addon;
        return NodeUtils.GetNodeById(&board->GetNodeById(nodeId)->GetAsAtkComponentRadioButton()->AtkComponentButton.AtkComponentBase, 3)->GetAsAtkNineGridNode()->AtkResNode.IsVisible();
    }

    public override IEnumerable<Item?> GetItems(nint addon) =>
        IsTabSelected(addon, ItemsNodeId)
            ? ContainerType.Inventory.GetContainer().Select(Item.FromInventoryItem)
            : [];
}
