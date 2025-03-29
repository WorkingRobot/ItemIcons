using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class ArmoireSimple : SingleIconProvider
{
    public override string Name => "Armoire (Simple)";

    public override string Description => "Shows an icon on items that can be stored in the Armoire.";

    public override BaseIcon Icon => new TextureIcon("ui/uld/ItemDetailPutIn.tex", new(36, 18, 18, 18));

    private readonly ImmutableSortedSet<uint> armoireItems;

    public ArmoireSimple()
    {
        armoireItems = LuminaSheets.CabinetSheet.Select(r => r.Item.RowId).ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        armoireItems.Contains(item.ItemId);
}
