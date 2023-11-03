using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class Marketable : SingleIconProvider
{
    public static string Name => "Marketable";

    public override BaseIcon Icon => new TextureIcon(65002);

    // https://github.com/Universalis-FFXIV/Universalis/blob/9d4693f5d0c02a31015337c6c10c97831f48f61a/src/Universalis.GameData/LuminaGameDataProvider.cs#L125
    public override bool Matches(Item item) =>
        item.LuminaRow.ItemSearchCategory.Row != 0 && !item.IsCollectible && !item.IsBinding;
}
