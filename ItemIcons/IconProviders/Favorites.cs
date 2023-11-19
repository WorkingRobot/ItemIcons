using Dalamud.ContextMenu;
using Dalamud.Game.Text;
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
        91152,
        91669,
        92150,
        92651,
        93067,
        93599,
        94167,
        94600,
    };

    public static readonly InventoryContextMenuItem ContextMenuItem;

    static Favorites()
    {
        var b = new SeStringBuilder();
        b.AddUiForeground(SeIconChar.BoxedLetterI.ToIconString(), 541);
        b.AddText(" Favorite");
        b.AddUiForegroundOff();
        ContextMenuItem = new(b.Build(), a =>
        {
            var item = new Item(a.ItemId);
            if (Service.Configuration.FavoritedItems.ContainsKey(item.ItemId))
                Service.Configuration.FavoritedItems.Remove(item.ItemId);
            else
                Service.Configuration.FavoritedItems[item.ItemId] = 0;
            Service.Configuration.Save();
        });
    }

    public Favorites()
    {
        var icons = IconIds.Select(id => (BaseIcon)new TextureIcon(id) { Scale = 5 / 3f, Offset = -6 });
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
