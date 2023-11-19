using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class Inventory : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Inventory;

    public override string AddonName => "Inventory";

    public override IEnumerable<string> GetDrawnAddonNames(nint addon) =>
        new[] { "InventoryGrid" };

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(3, 35).Select(i => GetDragDropIcon(drawnAddon, (uint)i));

    private static int? GetSelectedInventoryPage(nint addon)
    {
        var board = (AtkUnitBase*)addon;
        for (var i = 0; i < 4; ++i)
        {
            var id = (uint)i + 8;
            if (NodeUtils.GetNodeById(&board->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentBase, 3)->GetAsAtkNineGridNode()->AtkResNode.IsVisible)
                return i;
        }
        return null;
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var page = GetSelectedInventoryPage(addon);
        if (page == null)
            return Enumerable.Empty<Item?>();

        return ContainerType.Inventory.GetContainerPage(page.Value).Select(Item.FromInventoryItem);
    }
}
