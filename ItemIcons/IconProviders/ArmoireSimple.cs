using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class ArmoireSimple : SingleIconProvider
{
    public static string Name => "Armoire (Simple)";

    public override BaseIcon Icon => new TextureIcon("ui/uld/ItemDetailPutIn.tex", new(36, 18, 18, 18));

    private readonly ImmutableSortedSet<uint> armoireItems;

    public ArmoireSimple()
    {
        armoireItems = LuminaSheets.CabinetSheet.Select(r => r.Item.Row).ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        armoireItems.Contains(item.ItemId);
}
