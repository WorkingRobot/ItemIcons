using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.IconTypes;
using System.Collections.Generic;

namespace ItemIcons.IconProviders;

internal sealed class ItemLevel : IconProvider
{
    public override string Name => "Item Level";

    public override string Description => "Shows the item level of a given item.";

    public ItemLevel()
    {
        var icons = new List<BaseIcon>();
        var levelCount = LuminaSheets.ItemLevelSheet.RowCount;
        for (var i = 1; i < levelCount; i++)
        {
            icons.Add(new TextIcon()
            {
                Text = $"{i}",
                FontSize = 16,
                Alignment = AlignmentType.Center,
                Offset = 9,
                Flags = TextFlags.Glare | TextFlags.Edge,
                FontType = FontType.TrumpGothic
            });
        }

        Icons = icons;
        RegisterIcons();
    }

    public override uint? GetMatch(Item item)
    {
        if (item.LuminaRow.EquipSlotCategory.Row == 0)
            return null;

        var ilvl = item.LuminaRow.LevelItem.Row;
        if (ilvl != 0)
            return ResolveIconId(ilvl);

        return null;
    }

    private uint ResolveIconId(uint ilvl) =>
        IconOffset + ilvl - 1;
}
