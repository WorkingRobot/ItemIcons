using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System;
using System.Collections.Generic;

namespace ItemIcons.ItemProviders;

public abstract class BaseItemProvider : IDisposable
{
    public abstract ItemProviderCategory Category { get; }

    public abstract string AddonName { get; }

    public virtual IEnumerable<string> GetDrawnAddonNames(nint addon) =>
        new[] { AddonName };

    // Once SetupAddon is called on the addon, this should never return a different size unless you use InvalidateAddonCache
    public abstract IEnumerable<AtkItemIcon> GetIcons(nint drawnAddon);

    public abstract IEnumerable<Item?> GetItems(nint addon);

    public virtual void Dispose() { }

    public virtual unsafe void SetupAddon(nint drawnAddon)
    {
        foreach (var i in GetIcons(drawnAddon))
            i.Setup();
    }

    public virtual void FinalizeAddon(nint drawnAddon)
    {
        foreach (var i in GetIcons(drawnAddon))
            i.Destroy();
    }

    public static unsafe AtkItemIcon GetDragDropIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetDragDropIcon((AtkUnitBase*)addon, nodeId));

    public static unsafe AtkComponentIcon* GetDragDropIcon(AtkUnitBase* addon, uint id)
    {
        var dragDropNode = NodeUtils.GetAsAtkComponent<AtkComponentDragDrop>(addon->GetNodeById(id));
        return dragDropNode->AtkComponentIcon;
    }

    public static unsafe AtkItemIcon? TryGetDragDropIcon(nint addon, uint nodeId)
    {
        var a = (AtkUnitBase*)addon;
        var node = a->GetNodeById(nodeId);
        if (node == null)
            return null;
        var dragDropNode = NodeUtils.GetAsAtkComponent<AtkComponentDragDrop>(node);
        return AtkItemIcon.Create(dragDropNode->AtkComponentIcon);
    }

    public static unsafe AtkItemIcon GetBaseDragDropIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetBaseDragDropIcon((AtkUnitBase*)addon, nodeId));

    public static unsafe AtkComponentIcon* GetBaseDragDropIcon(AtkUnitBase* addon, uint id)
    {
        var node = NodeUtils.GetAsAtkComponent<AtkComponentBase>(addon->GetNodeById(id));
        var dragDropNode = NodeUtils.GetAsAtkComponent<AtkComponentDragDrop>(NodeUtils.GetNodeById(node, 5));
        return dragDropNode->AtkComponentIcon;
    }

    public static unsafe AtkItemIcon GetListIcon(nint listComponent, int idx)
    {
        var list = (AtkComponentList*)listComponent;
        var renderer = list->ItemRendererList[idx].AtkComponentListItemRenderer;
        return AtkItemIcon.Create(renderer);
    }

    public static unsafe AtkItemIcon GetCheckboxIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetCheckboxIcon((AtkUnitBase*)addon, nodeId));

    public static unsafe AtkComponentIcon* GetCheckboxIcon(AtkUnitBase* addon, uint id)
    {
        var node = NodeUtils.GetAsAtkComponent<AtkComponentCheckBox>(addon->GetNodeById(id));
        var iconNode = NodeUtils.GetAsAtkComponent<AtkComponentIcon>(NodeUtils.GetNodeById(&node->AtkComponentButton.AtkComponentBase, 31));
        return iconNode;
    }

    public static unsafe AtkItemIcon GetButtonIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetButtonIcon((AtkUnitBase*)addon, nodeId));

    public static unsafe AtkComponentButton* GetButtonIcon(AtkUnitBase* addon, uint id)
    {
        var buttonNode = NodeUtils.GetAsAtkComponent<AtkComponentButton>(addon->GetNodeById(id));
        return buttonNode;
    }

    public static unsafe AtkItemIcon GetComponentIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetComponentIcon((AtkUnitBase*)addon, nodeId));

    public static unsafe AtkComponentIcon* GetComponentIcon(AtkUnitBase* addon, uint id)
    {
        var iconNode = NodeUtils.GetAsAtkComponent<AtkComponentIcon>(addon->GetNodeById(id));
        return iconNode;
    }

    public static unsafe AtkItemIcon GetBaseButtonIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetBaseButtonIcon((AtkUnitBase*)addon, nodeId));

    public static unsafe AtkComponentButton* GetBaseButtonIcon(AtkUnitBase* addon, uint id)
    {
        var node = NodeUtils.GetAsAtkComponent<AtkComponentBase>(addon->GetNodeById(id));
        var buttonNode = NodeUtils.GetAsAtkComponent<AtkComponentButton>(NodeUtils.GetNodeById(node, 3));
        return buttonNode;
    }

    public static unsafe AtkItemIcon GetBaseIcon(nint addon, uint nodeId) =>
        AtkItemIcon.Create(GetBaseIcon(((AtkUnitBase*)addon)->GetNodeById(nodeId)));

    public static unsafe AtkItemIcon GetBaseIcon(nint node) =>
        AtkItemIcon.Create(GetBaseIcon((AtkResNode*)node));

    public static unsafe AtkComponentIcon* GetBaseIcon(AtkResNode* node)
    {
        var componentNode = NodeUtils.GetAsAtkComponent<AtkComponentBase>(node);
        var iconNode = NodeUtils.GetAsAtkComponent<AtkComponentIcon>(NodeUtils.GetNodeById(componentNode, 2));
        return iconNode;
    }
}
