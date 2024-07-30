using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class NeedGreed : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.Loot;

    public override string AddonName => "NeedGreed";

    private delegate nint OnRequestedUpdateDelegate(AddonNeedGreed* @this, NumberArrayData** a2, StringArrayData** a3);

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B 72 58 48 8B F9", DetourName = nameof(OnNeedGreedRequestedUpdate))]
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

    private nint OnNeedGreedRequestedUpdate(AddonNeedGreed* @this, NumberArrayData** a2, StringArrayData** a3)
    {
        var result = needGreedOnRequestedUpdateHook!.Original(@this, a2, a3);

        Service.Plugin.Renderer?.InvalidateAddonCache(AddonName);

        return result;
    }

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        var list = GetComponentList(drawnAddon);
        var length = GetListLength(list);

        return Enumerable.Range(0, length).Select(i => GetListIcon(list, i));
    }

    private static nint GetComponentList(nint drawnAddon) =>
        (nint)NodeUtils.GetAsAtkComponent<AtkComponentList>(((AtkUnitBase*)drawnAddon)->GetNodeById(6));

    private static int GetListLength(nint listComponent) =>
        ((AtkComponentList*)listComponent)->ListLength;

    private static LootItem GetLootItem(int idx) =>
        Loot.Instance()->Items[idx];

    public override IEnumerable<Item?> GetItems(nint addon) =>
        Enumerable.Range(0, 16).Select(i => (Item?)new Item(GetLootItem(i).ItemId));
}
