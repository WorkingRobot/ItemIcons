using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class VendorItem : SingleIconProvider
{
    public override string Name => "Vendor Item";

    public override string Description => "Shows an icon on items that can be traded with vendors to recieve other goodies. (e.g. EX totems, Cracked Dendroclusters, Irregular Tomestones, etc.))";

    public override BaseIcon Icon => new TextureIcon(60412) { Scale = 1.75f, Offset = -8 };

    private readonly ImmutableSortedSet<uint> vendorItems;

    public VendorItem()
    {
        vendorItems = LuminaSheets.SpecialShopSheet
            .SelectMany(s => s.Item)                // Get shop entries
            .SelectMany(i => i.ItemCosts)           // Get entry costs
            .Where(i => i.ItemCost.IsValid)         // Item is valid
            .Select(i => i.ItemCost.RowId)          // Get item id
            .Distinct()                             // Remove duplicates
            .Select(LuminaSheets.ItemSheet.GetRow)  // Get item row
            .Where(l =>
                !l.Name.IsEmpty &&                  // Item has a name
                l.EquipSlotCategory.RowId == 0 &&   // Item is not equippable
                l.ItemUICategory.RowId != 100       // Item is not a currency
            )
            .Select(l => l!.RowId)
            .ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        vendorItems.Contains(item.ItemId);
}
