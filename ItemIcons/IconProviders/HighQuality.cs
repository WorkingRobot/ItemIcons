using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class HighQuality : SingleIconProvider
{
    public override string Name => "High Quality";

    public override BaseIcon Icon => new TextureIcon("ui/uld/Synthesis_hr1.tex", new(0, 240, 36, 36));

    public override bool Matches(Item item) =>
        item.IsHq;
}
