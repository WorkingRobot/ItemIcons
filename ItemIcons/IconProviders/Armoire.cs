using ItemIcons.IconTypes;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.IconProviders;

internal sealed class Armoire : IconProvider
{
    public override string Name => "Armoire (Icons)";

    public override string Description => "Shows the specific Armoire category that an item can be stored in.";

    private readonly Dictionary<uint, int> categoryLut = [];

    public Armoire()
    {
        var icons = new BaseIcon[8];
        for (var i = 0; i < 8; ++i)
        {
            var categoryRow = (uint)(i + 1);
            var category = LuminaSheets.CabinetCategorySheet.GetRow(categoryRow)!;
            foreach (var item in LuminaSheets.CabinetSheet.Where(item => item.Category.Row == categoryRow).Select(i => i.Item.Row))
                categoryLut.TryAdd(item, i);
            icons[i] = new TextureIcon((uint)category.Icon);
        }

        Icons = icons;
        RegisterIcons();
    }

    public override uint? GetMatch(Item item)
    {
        if (categoryLut.TryGetValue(item.ItemId, out var categoryIdx))
            return (uint)(IconOffset + categoryIdx);
        return null;
    }
}
