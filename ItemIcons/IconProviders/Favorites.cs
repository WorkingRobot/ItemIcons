using Dalamud.Game.Text.SeStringHandling;
using ItemIcons.IconTypes;
using System.Linq;

namespace ItemIcons.IconProviders;

internal sealed class Favorites : IconProvider
{
    public override string Name => "Favorites";

    public override string Description => "User-selected favorites! Pick and choose which items you want to favorite! (Works based on the item, not the actual item stack or inventory slot)";

    private static readonly uint[] IconIds = new uint[]
    {
        60651,
        60652,
        60653,
        60654,
        60655,
        60656,
    };
    
    public static readonly BitmapFontIcon[] BitmapIconIds = new BitmapFontIcon[]
    {
        BitmapFontIcon.ElementFire,
        BitmapFontIcon.ElementIce,
        BitmapFontIcon.ElementWind,
        BitmapFontIcon.ElementEarth,
        BitmapFontIcon.ElementLightning,
        BitmapFontIcon.ElementWater,
    };

    public Favorites()
    {
        var icons = IconIds.Select(id => (BaseIcon)new TextureIcon(id) { Scale = 4 / 3f, Offset = -3 });
        Icons = icons.ToList();
        RegisterIcons();
    }

    public override uint? GetMatch(Item item)
    {
        if (Service.Configuration.FavoritedItems.TryGetValue(item.ItemId, out var iconIdx))
            return IconOffset + iconIdx;

        return null;
    }
}
