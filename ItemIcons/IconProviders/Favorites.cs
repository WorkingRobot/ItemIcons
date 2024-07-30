using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.IconTypes;
using System.Linq;

namespace ItemIcons.IconProviders;

internal sealed class Favorites : IconProvider
{
    public override string Name => "Favorites";

    public override string Description => "User-selected favorites! Pick and choose which items you want to favorite! (Works based on the item, not the actual item stack or inventory slot)";
    
    public static readonly BitmapFontIcon[] IconIds =
    [
        BitmapFontIcon.ElementFire,
        BitmapFontIcon.ElementIce,
        BitmapFontIcon.ElementWind,
        BitmapFontIcon.ElementEarth,
        BitmapFontIcon.ElementLightning,
        BitmapFontIcon.ElementWater,
    ];

    public Favorites()
    {
        var icons = IconIds.Select(id => (BaseIcon)new TextIcon()
        {
            Text = new SeStringBuilder().AddIcon(id).Build(),
            FontSize = 16,
            Alignment = AlignmentType.Center,
            Offset = 9,
            Flags = TextFlags.Glare | TextFlags.Edge
        });
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
