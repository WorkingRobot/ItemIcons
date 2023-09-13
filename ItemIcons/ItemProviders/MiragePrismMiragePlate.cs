using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.Agents;
using ItemIcons.AtkIcons;
using System.Collections.Generic;
using System.Linq;
using static ItemIcons.Agents.MiragePlateItem;

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
            var slotIdx = item.Slot switch
            {
                SlotType.Offhand => 0,
                SlotType.Mainhand => 1,
                SlotType.Head => 2,
                SlotType.Body => 3,
                SlotType.Hands => 4,
                SlotType.Legs => 5,
                SlotType.Feet => 6,
                SlotType.Ears => 7,
                SlotType.Neck => 8,
                SlotType.Wrist => 9,
                SlotType.RingRight => 10,
                SlotType.RingLeft => 11,
                _ => -1,
            };
            if (slotIdx == -1)
                continue;
            slots[slotIdx] = new(item);
        }

        return slots;
    }
}
