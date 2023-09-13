using System.Collections.Immutable;
using System.Linq;
using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal sealed class VendorItem : SingleIconProvider
{
    public override string Name => "Vendor Item";

    public override BaseIcon Icon => new TextureIcon(60412) { Scale = 1.75f, Offset = -8 };

    private readonly ImmutableSortedSet<uint> vendorItems;

    public VendorItem()
    {
        vendorItems = LuminaSheets.SpecialShopSheet
            .SelectMany(s => s.Entries)             // Get shop entries
            .SelectMany(i => i.Cost)                // Get entry costs
            .Select(i => i.Item.Row)                // Get item id
            .Distinct()                             // Remove duplicates
            .Select(LuminaSheets.ItemSheet.GetRow)  // Get item row
            .Where(l =>
                l != null &&                        // Item is valid
                !l.Name.RawData.IsEmpty &&          // Item has a name
                l.EquipSlotCategory.Row == 0 &&     // Item is not equippable
                l.ItemUICategory.Row != 100         // Item is not a currency
            )
            .Select(l => l!.RowId)
            .ToImmutableSortedSet();
    }

    public override bool Matches(Item item) =>
        vendorItems.Contains(item.ItemId);
}
