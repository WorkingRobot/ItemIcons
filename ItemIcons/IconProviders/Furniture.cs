using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class Furniture : SingleIconProvider
{
    public override string Name => "Furniture";

    public override string Description => "Shows an icon on items that are used as furniture.";

    public override BaseIcon Icon => new TextureIcon(60752) { Scale = 1.5f, Offset = -6};

    private readonly ImmutableSortedSet<uint> furnitureItems;

    public Furniture()
    {
        furnitureItems = LuminaSheets.HousingFurnitureSheet.Select(f => f.Item.Row).ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        furnitureItems.Contains(item.ItemId);
}
