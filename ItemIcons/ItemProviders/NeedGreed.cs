using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class NeedGreed : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Loot;

    public override string AddonName => "NeedGreed";

    private delegate nint OnRequestedUpdateDelegate(nint a1, nint a2, nint a3);

    [Signature("40 53 48 83 EC 20 48 8B 42 58", DetourName = nameof(OnNeedGreedRequestedUpdate))]
    private readonly Hook<OnRequestedUpdateDelegate> needGreedOnRequestedUpdateHook = null!;

    public NeedGreed()
    {
        Service.GameInteropProvider.InitializeFromAttributes(this);
        needGreedOnRequestedUpdateHook.Enable();
    }

    public override void Dispose()
    {
        needGreedOnRequestedUpdateHook?.Dispose();
    }

    private nint OnNeedGreedRequestedUpdate(nint addon, nint a2, nint a3)
    {
        var result = needGreedOnRequestedUpdateHook!.Original(addon, a2, a3);

        Service.Plugin.Renderer?.InvalidateAddonCache(AddonName);

        return result;
    }

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        var list = GetComponentList(drawnAddon);
        var length = GetListLength(list);

        for (var i = 0; i < length; ++i)
            yield return GetListIcon(list, i);
    }

    private static nint GetComponentList(nint drawnAddon) =>
        (nint)NodeUtils.GetAsAtkComponent<AtkComponentList>(((AtkUnitBase*)drawnAddon)->GetNodeById(6));

    private static int GetListLength(nint listComponent) =>
        ((AtkComponentList*)listComponent)->ListLength;

    private static LootItem GetLootItem(int idx) =>
        Loot.Instance()->ItemArraySpan[idx];

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        for (var i = 0; i < 16; ++i)
            yield return new Item(GetLootItem(i).ItemId);
    }
}
