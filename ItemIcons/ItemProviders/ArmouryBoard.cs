using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class ArmouryBoard : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.ArmouryBoard;

    public override string AddonName => "ArmouryBoard";

    private static readonly Dictionary<uint, ContainerType> TabToInventoryType = new()
    {
        [7] = ContainerType.ArmoryMainHand,
        [8] = ContainerType.ArmoryHead,
        [9] = ContainerType.ArmoryBody,
        [10] = ContainerType.ArmoryHands,
        [11] = ContainerType.ArmoryLegs,
        [12] = ContainerType.ArmoryFeets,
        [13] = ContainerType.ArmoryOffHand,
        [14] = ContainerType.ArmoryEar,
        [15] = ContainerType.ArmoryNeck,
        [16] = ContainerType.ArmoryWrist,
        [17] = ContainerType.ArmoryRings,
        [18] = ContainerType.ArmorySoulCrystal,
    };

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        for(uint i = 71; i < 121; ++i)
            yield return GetDragDropIcon(drawnAddon, i);
    }
    
    private static ContainerType? GetSelectedInventoryType(nint addon)
    {
        var board = (AtkUnitBase*)addon;
        foreach(var (id, type) in TabToInventoryType)
        {
            if (NodeUtils.GetNodeById(&board->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentBase, 6)->GetAsAtkImageNode()->AtkResNode.IsVisible)
                return type;
        }
        return null;
    }

    public override IEnumerable<Item?> GetItems(nint addon) =>
        GetSelectedInventoryType(addon)?.GetContainer()?.Select(Item.FromInventoryItem) ?? Enumerable.Empty<Item?>();
}
