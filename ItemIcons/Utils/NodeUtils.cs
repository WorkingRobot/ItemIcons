using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

namespace ItemIcons.Utils;

internal static unsafe class NodeUtils
{
    public static AtkComponentNode* GetAsAtkComponentNode(AtkResNode* me) =>
        (ushort)me->Type < 1000
            ? throw new ArgumentException("Node is not a component node", nameof(me))
            : (AtkComponentNode*)me;

    public static T* GetComponent<T>(AtkComponentNode* node) where T : unmanaged =>
        GetNodeComponentType(node) != GetComponentType(typeof(T))
            ? throw new ArgumentException($"{GetNodeComponentType(node)} node is not a {typeof(T).Name}", nameof(node))
            : (T*)node->Component;

    public static T* GetAsAtkComponent<T>(AtkResNode* me) where T : unmanaged =>
        GetComponent<T>(GetAsAtkComponentNode(me));

    private static ComponentType GetComponentType(Type t)
    {
        if (t == typeof(AtkComponentBase)) return ComponentType.Base;
        if (t == typeof(AtkComponentButton)) return ComponentType.Button;
        if (t == typeof(AtkComponentWindow)) return ComponentType.Window;
        if (t == typeof(AtkComponentCheckBox)) return ComponentType.CheckBox;
        if (t == typeof(AtkComponentRadioButton)) return ComponentType.RadioButton;
        if (t == typeof(AtkComponentGaugeBar)) return ComponentType.GaugeBar;
        if (t == typeof(AtkComponentSlider)) return ComponentType.Slider;
        if (t == typeof(AtkComponentTextInput)) return ComponentType.TextInput;
        if (t == typeof(AtkComponentNumericInput)) return ComponentType.NumericInput;
        if (t == typeof(AtkComponentList)) return ComponentType.List;
        if (t == typeof(AtkComponentDropDownList)) return ComponentType.DropDownList;
        // if (t == typeof(AtkComponentTab)) return ComponentType.Tab;
        if (t == typeof(AtkComponentTreeList)) return ComponentType.TreeList;
        if (t == typeof(AtkComponentScrollBar)) return ComponentType.ScrollBar;
        if (t == typeof(AtkComponentListItemRenderer)) return ComponentType.ListItemRenderer;
        if (t == typeof(AtkComponentIcon)) return ComponentType.Icon;
        if (t == typeof(AtkComponentIconText)) return ComponentType.IconText;
        if (t == typeof(AtkComponentDragDrop)) return ComponentType.DragDrop;
        if (t == typeof(AtkComponentGuildLeveCard)) return ComponentType.GuildLeveCard;
        if (t == typeof(AtkComponentTextNineGrid)) return ComponentType.TextNineGrid;
        if (t == typeof(AtkComponentJournalCanvas)) return ComponentType.JournalCanvas;
        // if (t == typeof(AtkComponentMultipurpose)) return ComponentType.Multipurpose;
        // if (t == typeof(AtkComponentMap)) return ComponentType.Map;
        // if (t == typeof(AtkComponentPreview)) return ComponentType.Preview;
        if (t == typeof(AtkComponentHoldButton)) return ComponentType.HoldButton;
        if (t == typeof(AtkComponentPortrait)) return ComponentType.Portrait;
        throw new ArgumentOutOfRangeException(nameof(t), t, "Unknown component type");
    }

    private static ComponentType? GetNodeComponentType(AtkComponentNode* node)
    {
        var info = ((AtkUldComponentInfo*)node->Component->UldManager.Objects);
        return info == null ? null : info->ComponentType;
    }

    public static AtkResNode* GetNodeById(AtkComponentBase* node, uint id) =>
        node->UldManager.SearchNodeById(id);

    public static AtkResNode* GetNodeByIdStrict(AtkComponentBase* node, uint id)
    {
        for (var i = 0; i < node->UldManager.NodeListCount; i++)
        {
            var n = node->UldManager.NodeList[i];
            if (n->NodeID == id)
                return n;
        }
        return null;
    }

    public static void SetVisibility(AtkResNode* node, bool visible)
    {
        if (visible)
            node->NodeFlags |= NodeFlags.Visible;
        else
            node->NodeFlags &= ~NodeFlags.Visible;
    }
}
