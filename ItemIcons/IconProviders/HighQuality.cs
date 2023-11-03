using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class HighQuality : SingleIconProvider
{
    public override string Name => "High Quality";

    public override string Description => "Shows an HQ icon on HQ items.";

    public override BaseIcon Icon => new TextureIcon("ui/uld/Synthesis.tex", new(0, 120, 18, 18));

    public override bool Matches(Item item) =>
        item.IsHq;
}
