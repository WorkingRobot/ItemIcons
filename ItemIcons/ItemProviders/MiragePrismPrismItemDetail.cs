using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.AtkIcons;
using System.Collections.Generic;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class MiragePrismPrismItemDetail : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.ItemDetail;

    public override string AddonName => "MiragePrismPrismItemDetail";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        yield return GetComponentIcon(drawnAddon, 6);
    }

    private static uint? GetItemId()
    {
        var agent = AgentMiragePrismPrismItemDetail.Instance();
        if (agent == null)
            return null;

        return agent->ItemId;
    }
    
    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var item = GetItemId();
        if (item == null)
            yield break;

        yield return new Item(item.Value);
    }
}
