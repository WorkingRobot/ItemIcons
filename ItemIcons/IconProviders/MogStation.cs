using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class MogStation : SingleIconProvider
{
    public override string Name => "Mog Station";

    public override string Description => "Shows an icon on items that are available for purchase on the Mog Station.";

    public override BaseIcon Icon => new TextureIcon("ui/uld/LetterList3.tex", new(0, 0, 32, 32));

    private readonly ImmutableSortedSet<uint> mogStationItems;

    public MogStation()
    {
        mogStationItems = Supplemental.StoreItems.Select(i => i.ItemId).ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        mogStationItems.Contains(item.ItemId);
}
