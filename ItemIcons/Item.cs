using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using LuminaItem = Lumina.Excel.Sheets.Item;

namespace ItemIcons;

public readonly record struct Item
{
    public uint ItemId { get; init; }
    public bool IsHq { get; init; }
    public bool IsCollectible { get; init; }
    public byte[] Stains { get; init; }

    // Is Collectability if IsCollectible
    public ushort? Spiritbond { get; init; }
    public ushort? Condition { get; init; }
    public ushort[]? Materia { get; init; }
    public byte[]? MateriaGrade { get; init; }

    public bool IsBinding => Spiritbond.HasValue && (Spiritbond * 100) != 0;

    public LuminaItem LuminaRow => LuminaSheets.ItemSheet.GetRow(ItemId)!;

    public unsafe Item(InventoryItem item)
    {
        ItemId = item.ItemId;
        IsHq = item.Flags.HasFlag(InventoryItem.ItemFlags.HighQuality);
        IsCollectible = item.Flags.HasFlag(InventoryItem.ItemFlags.Collectable);
        Stains = item.Stains.ToArray();

        Spiritbond = item.SpiritbondOrCollectability;
        Condition = item.Condition;
        Materia = item.Materia.ToArray();
        MateriaGrade = item.MateriaGrades.ToArray();
    }

    public Item(PrismBoxItem item)
    {
        ItemId = item.ItemId;
        Stains = item.Stains.ToArray();

        if (IsHq = ItemId > 1000000)
            ItemId -= 1000000;
        if (IsCollectible = ItemId > 500000)
            ItemId -= 500000;
    }

    public Item(CharaViewItem item)
    {
        ItemId = item.ItemId;
        Stains = [item.Stain0Id, item.Stain1Id];
    }

    public Item(uint itemId)
    {
        ItemId = itemId;

        if (IsHq = ItemId > 1000000)
            ItemId -= 1000000;
        if (IsCollectible = ItemId > 500000)
            ItemId -= 500000;

        Stains = [];
    }

    public static Item? FromInventoryItem(InventoryItem? item) =>
        item == null ? null : new(item.Value);

    public static Item? FromPrismBoxItem(PrismBoxItem? item) =>
        item == null ? null : new(item.Value);

    public static Item? FromMiragePlateItem(CharaViewItem? item) =>
        item == null ? null : new(item.Value);
}
