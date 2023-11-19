using FFXIVClientStructs.FFXIV.Client.Game;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class CharacterInspect : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Character;

    public override string AddonName => "CharacterInspect";

    private static readonly uint[] IconOrder = new uint[]
    {
        12, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47
    };

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        foreach (var i in IconOrder)
            yield return GetButtonIcon(drawnAddon, i);
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var container = InventoryType.Examine.GetContainer();
        
        //               Skip belt slot
        return container.Where((i, idx) => idx != 5).Select(Item.FromInventoryItem);
    }
}
