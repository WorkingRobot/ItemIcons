using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.Agents;
using System;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;

namespace ItemIcons;

public readonly record struct Item
{
    public uint ItemId { get; init; }
    public bool IsHq { get; init; }
    public bool IsCollectible { get; init; }
    public byte Stain { get; init; }

    // Is Collectability if IsCollectible
    public ushort? Spiritbond { get; init; }
    public ushort? Condition { get; init; }
    public ushort[]? Materia { get; init; }
    public byte[]? MateriaGrade { get; init; }

    public bool IsBinding => Spiritbond.HasValue && (Spiritbond * 100) != 0;

    public LuminaItem LuminaRow => LuminaSheets.ItemSheet.GetRow(ItemId)!;

    public unsafe Item(InventoryItem item)
    {
        ItemId = item.ItemID;
        IsHq = item.Flags.HasFlag(InventoryItem.ItemFlags.HQ);
        IsCollectible = item.Flags.HasFlag(InventoryItem.ItemFlags.Collectable);
        Stain = item.Stain;

        Spiritbond = item.Spiritbond;
        Condition = item.Condition;
        Materia = new Span<ushort>(item.Materia, 5).ToArray();
        MateriaGrade = new Span<byte>(item.MateriaGrade, 5).ToArray();
    }

    public Item(PrismBoxItem item)
    {
        ItemId = item.ItemId;
        Stain = item.Stain;

        if (IsHq = ItemId > 1000000)
            ItemId -= 1000000;
        if (IsCollectible = ItemId > 500000)
            ItemId -= 500000;
    }

    public Item(MiragePlateItem item)
    {
        ItemId = item.ItemId;
        Stain = item.Stain;
    }

    public Item(uint itemId)
    {
        ItemId = itemId;

        if (IsHq = ItemId > 1000000)
            ItemId -= 1000000;
        if (IsCollectible = ItemId > 500000)
            ItemId -= 500000;
    }

    public static Item? FromInventoryItem(InventoryItem? item) =>
        item == null ? null : new(item.Value);

    public static Item? FromPrismBoxItem(PrismBoxItem? item) =>
        item == null ? null : new(item.Value);

    public static Item? FromMiragePlateItem(MiragePlateItem? item) =>
        item == null ? null : new(item.Value);
}
