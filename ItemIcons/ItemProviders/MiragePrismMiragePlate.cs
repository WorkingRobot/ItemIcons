using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.AtkIcons;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

public enum MiragePlateItemSlotType : byte
{
    Mainhand,
    Offhand,
    Head,
    Body,
    Hands,
    Waist,
    Legs,
    Feet,
    Ears,
    Neck,
    Wrist,
    RingRight,
    RingLeft,
    Empty = 14
}

internal sealed unsafe class MiragePrismMiragePlate : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.MiragePlate;

    public override string AddonName => "MiragePrismMiragePlate";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(63, 12).Select(i => GetBaseButtonIcon(drawnAddon, (uint)i));

    private static MiragePlateItem[]? GetMiragePlateItems()
    {
        var agent = AgentMiragePrismMiragePlate.Instance();
        if (agent == null)
            return null;

        return agent->PlateItemsSpan.ToArray();
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var items = GetMiragePlateItems();
        if (items == null)
            return Enumerable.Empty<Item?>();

        var slots = new Item?[12];
        foreach (var item in items)
        {
            var slotIdx = (MiragePlateItemSlotType)item.EquipType switch
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
