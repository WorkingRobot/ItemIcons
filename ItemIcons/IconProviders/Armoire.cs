using ItemIcons.IconTypes;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.IconProviders;

internal sealed class Armoire : IconProvider
{
    public override string Name => "Armoire (Icons)";

    private uint IdOffset { get; }

    private readonly Dictionary<uint, int> categoryLut = new();

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

        IdOffset = RegisterIcons(icons);
        Log.Debug($"Registering {GetType().Name} to {IdOffset}");
    }

    public override uint? GetMatch(Item item)
    {
        if (categoryLut.TryGetValue(item.ItemId, out var categoryIdx))
            return (uint)(IdOffset + categoryIdx);
        return null;
    }
}
