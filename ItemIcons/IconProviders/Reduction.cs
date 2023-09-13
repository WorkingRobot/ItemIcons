using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class Reduction : SingleIconProvider
{
    public override string Name => "Reducible";

    public override BaseIcon Icon => new TextureIcon(121) { Scale = .8f, Offset = 2};

    public override bool Matches(Item item) =>
        item.IsCollectible && item.LuminaRow.AetherialReduce != 0;
}
