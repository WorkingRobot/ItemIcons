using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class Tradeable : SingleIconProvider
{
    public override string Name => "Tradeable";

    public override BaseIcon Icon => new TextureIcon(60935) { Scale = 1.75f, Offset = -8 };

    private readonly ImmutableSortedSet<uint> tradeableItems;

    public Tradeable()
    {
        tradeableItems = LuminaSheets.ItemSheet.Where(i => !i.IsUntradable && i.ItemSearchCategory.Row != 0).Select(i => i.RowId).ToImmutableSortedSet();
    }

    // https://github.com/Critical-Impact/CriticalCommonLib/blob/bde61b016c0c587fc4ebd578c2e864828ee5b919/Models/InventoryItem.cs#L187
    public override bool Matches(Item item) =>
        tradeableItems.Contains(item.ItemId) && !item.IsCollectible && !item.IsBinding;
}
