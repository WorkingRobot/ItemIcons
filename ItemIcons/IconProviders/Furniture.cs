using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class Furniture : SingleIconProvider
{
    public static string Name => "Furniture";

    public override BaseIcon Icon => new TextureIcon(60752) { Scale = 1.5f, Offset = -6};

    private readonly ImmutableSortedSet<uint> furnitureItems;

    public Furniture()
    {
        furnitureItems = LuminaSheets.HousingFurnitureSheet.Select(f => f.Item.Row).ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        furnitureItems.Contains(item.ItemId);
}
