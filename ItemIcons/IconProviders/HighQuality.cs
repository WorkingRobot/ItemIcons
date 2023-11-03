using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class HighQuality : SingleIconProvider
{
    public static string Name => "High Quality";

    public override BaseIcon Icon => new TextureIcon("ui/uld/Synthesis.tex", new(0, 120, 18, 18));

    public override bool Matches(Item item) =>
        item.IsHq;
}
