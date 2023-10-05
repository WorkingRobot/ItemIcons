using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.AtkIcons;
using System.Collections.Generic;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class ItemDetail : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.ItemDetail;

    public override string AddonName => "ItemDetail";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        yield return GetComponentIcon(drawnAddon, 31);
    }

    private static uint? GetItemId()
    {
        var agent = AgentItemDetail.Instance();
        if (agent == null)
            return null;

        return agent->ItemId;
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var item = GetItemId();
        if (item == null)
            yield break;

        // No event items
        if (item > 2000000)
            yield return new Item(0);
        else
            yield return new Item(item.Value);
    }
}
