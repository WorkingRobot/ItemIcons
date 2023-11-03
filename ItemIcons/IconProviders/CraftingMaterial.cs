using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class CraftingMaterial : SingleIconProvider
{
    public override string Name => "Crafting Material";

    public override string Description => "Shows an icon on items that are used as crafting materials.";

    public override BaseIcon Icon => new TextureIcon("ui/uld/ToggleButton.tex", new(104, 39, 18, 18)) { Scale = 1.5f, Offset = -6 };

    private readonly ImmutableSortedSet<uint> craftingMaterials;

    public CraftingMaterial()
    {
        craftingMaterials = LuminaSheets.RecipeSheet
            .SelectMany(r => r.UnkData5)
            .Select(i => (uint)i.ItemIngredient)
            .ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        craftingMaterials.Contains(item.ItemId);
}
