using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class ItemLevel : SingleIconProvider
{
    public override string Name => "ILvl 1 (Debug)";

    public override BaseIcon Icon => new TextureIcon("ui/uld/RecipeNoteBook_hr1.tex", new(64, 30, 20, 20));

    public override bool Matches(Item item) =>
        item.LuminaRow.LevelEquip == 1;
}
