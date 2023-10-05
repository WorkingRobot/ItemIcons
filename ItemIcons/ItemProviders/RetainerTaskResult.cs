using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using System.Collections.Generic;

namespace ItemIcons.ItemProviders;

internal sealed unsafe class RetainerTaskResult : BaseItemProvider
{
    public override ItemProviderCategory Category => ItemProviderCategory.RetainerVenture;

    public override string AddonName => "RetainerTaskResult";

    private delegate AtkResNode* GetDuplicatedNodeDelegate(AtkUldManager* uldManager, int nodeId, int idx, int offset);

    [Signature("45 33 DB 41 8B C1 44 8B D2 4C 8B C9 45 85 C0 74")]
    private readonly GetDuplicatedNodeDelegate getDuplicatedNode = null!;

    public RetainerTaskResult() =>
        Service.GameInteropProvider.InitializeFromAttributes(this);

    private static nint GetUldManager(nint drawnAddon)
    {
        var addon = (AtkUnitBase*)drawnAddon;
        return (nint)(&addon->UldManager);
    }

    private nint GetDuplicatedNode(nint uldManager, int nodeId, int idx, int offset)
    {
        var node = getDuplicatedNode((AtkUldManager*)uldManager, nodeId, idx, offset);
        return (nint)node;
    }

    public override IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon)
    {
        var uldManager = GetUldManager(drawnAddon);
        for (var i = 0; i < 2; ++i)
        {
            var v = GetDuplicatedNode(uldManager, 13, i, 100);
            if (v != nint.Zero)
                yield return GetBaseIcon(v);
        }
    }

    private static (uint? ItemA, uint? ItemB) GetItemIds()
    {
        var agent = AgentRetainerTask.Instance();
        if (agent == null)
            return (null, null);

        if (agent->DisplayType != 3)
            return (null, null);

        return (agent->RewardItemIds[0], agent->RewardItemIds[1]);
    }

    public override IEnumerable<Item?> GetItems(nint addon)
    {
        var (a, b) = GetItemIds();

        if (a.HasValue && a != 0)
            yield return new Item(a.Value);
        if (b.HasValue && b != 0)
            yield return new Item(b.Value);
    }
}
