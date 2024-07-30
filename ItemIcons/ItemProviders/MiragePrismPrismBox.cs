using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ItemIcons.AtkIcons;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class MiragePrismPrismBox : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.MirageBox;

    public override string AddonName => "MiragePrismPrismBox";

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon) =>
        Enumerable.Range(31, 50).Select(i => GetButtonIcon(drawnAddon, (uint)i));

    private static nint? GetMirageData()
    {
        var agent = AgentMiragePrismPrismBox.Instance();
        if (agent == null)
            return null;

        var data = agent->Data;
        if (data == null)
            return null;

        return (nint)data;
    }

    private static PrismBoxItem? GetMirageItem(nint data, int idx)
    {
        var dataPtr = (MiragePrismPrismBoxData*)data;
        var i = dataPtr->PageItemIndexes[idx];
        if (i >= dataPtr->PrismBoxItems.Length)
            return null;
        var item = dataPtr->PrismBoxItems[i];
        return item;
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var data = GetMirageData();
        if (data == null)
            yield break;
        for (var i = 0; i < 50; ++i)
            yield return Item.FromPrismBoxItem(GetMirageItem(data.Value, i));
    }
}
