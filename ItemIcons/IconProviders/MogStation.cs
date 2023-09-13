using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class MogStation : SingleIconProvider
{
    public override string Name => "Mog Station";

    public override BaseIcon Icon => new TextureIcon("ui/uld/LetterList3_hr1.tex", new(0, 0, 64, 64));

    private readonly ImmutableSortedSet<uint> mogStationItems;

    public MogStation()
    {
        mogStationItems = Supplemental.StoreItems.Select(i => i.ItemId).ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        mogStationItems.Contains(item.ItemId);
}
