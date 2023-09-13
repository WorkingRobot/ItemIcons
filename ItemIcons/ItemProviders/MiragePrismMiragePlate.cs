using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.Agents;
using ItemIcons.AtkIcons;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class MiragePrismMiragePlate : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.MiragePlate;

    public override string AddonName => "MiragePrismMiragePlate";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        for (uint i = 63; i < 75; ++i)
            yield return GetBaseButtonIcon(drawnAddon, i);
    }

    private static MiragePlateItem[]? GetMiragePlateItems()
    {
        var agent = Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.MiragePrismMiragePlate);
        if (agent == null)
            return null;

        return ((AgentMiragePrismMiragePlate*)agent)->PlateItemsSpan.ToArray();
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var items = GetMiragePlateItems();
        if (items == null)
            return Enumerable.Empty<Item?>();

        var slots = new Item?[12];
        foreach (var item in items)
        {
            var slotIdx = (MiragePlateItemSlotType)item.EquipSlotCategory switch
            {
                MiragePlateItemSlotType.Offhand => 0,
                MiragePlateItemSlotType.Mainhand => 1,
                MiragePlateItemSlotType.Head => 2,
                MiragePlateItemSlotType.Body => 3,
                MiragePlateItemSlotType.Hands => 4,
                MiragePlateItemSlotType.Legs => 5,
                MiragePlateItemSlotType.Feet => 6,
                MiragePlateItemSlotType.Ears => 7,
                MiragePlateItemSlotType.Neck => 8,
                MiragePlateItemSlotType.Wrist => 9,
                MiragePlateItemSlotType.RingRight => 10,
                MiragePlateItemSlotType.RingLeft => 11,
                _ => -1,
            };
            if (slotIdx == -1)
                continue;
            slots[slotIdx] = new(item);
        }

        return slots;
    }
}
