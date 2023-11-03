using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.IconProviders;
using ItemIcons.Utils;
using System;

namespace ItemIcons.AtkIcons;

public abstract unsafe class AtkItemIcon
{
    public AtkResNode* Node { get; init; }

    protected const int ImageNode1Id = 1000;
    protected const int ImageNode2Id = 1001;
    protected const int TextNode1Id = 1002;
    protected const int TextNode2Id = 1003;

    public AtkImageNode* ImageNode1 { get; protected set; }
    public AtkImageNode* ImageNode2 { get; protected set; }
    public AtkTextNode* TextNode1 { get; protected set; }
    public AtkTextNode* TextNode2 { get; protected set; }

    public abstract void Setup();

    public abstract void Destroy();

    public virtual void UpdateDirtyNode(AtkResNode* node)
    {
        // 0x1 = IsDirty
        // 0x2 = AreChildrenDirty
        // 0x4 = IsTransformDirty
        // 0x8 = AreChildrenTransformDirty
        node->ViewFlags |= (NodeViewFlags)0x5;
    }

    public void SetIcons(ReadOnlySpan<uint> iconIds, ulong msTimestamp)
    {
        ClearIcon();
        for(var i = 0; i < iconIds.Length; i++)
        {
            var (isPrimary, isSecondary, alpha) = GetAlphaForIcon(i, iconIds.Length, msTimestamp);
            if (!isPrimary && !isSecondary)
                continue;
            if (IconProvider.GetIcon(iconIds[i]) is { } icon)
                icon.Apply(this, isPrimary, alpha);
        }
    }

    public void ClearIcon()
    {
        Setup();
        ImageNode1->AtkResNode.Color.A = 0;
        ImageNode2->AtkResNode.Color.A = 0;
        TextNode1->AtkResNode.Color.A = 0;
        TextNode2->AtkResNode.Color.A = 0;
        NodeUtils.SetVisibility(&ImageNode1->AtkResNode, false);
        NodeUtils.SetVisibility(&ImageNode2->AtkResNode, false);
        NodeUtils.SetVisibility(&TextNode1->AtkResNode, false);
        NodeUtils.SetVisibility(&TextNode2->AtkResNode, false);
    }

    public static (bool IsPrimary, bool IsSecondary, byte Alpha) GetAlphaForIcon(int idx, int iconCount, ulong msTimestamp)
    {
        if (iconCount == 1)
            return (true, false, 255);

        const int ShownDuration = 2000;
        const int SwapDuration = 500;
        const int AnimDuration = ShownDuration + SwapDuration;
        var fullDuration = AnimDuration * iconCount;
        // Count: 3
        // 0-2000: 0
        // 2000-2500: 0->1
        // 2500-4500: 1
        // 4500-5000: 1->2
        // 5000-7000: 2
        // 7000-7500: 2->0
        // Repeat...
        var animTime = (int)(msTimestamp % (ulong)fullDuration);
        var (activeIcon, iconTime) = Math.DivRem(animTime, AnimDuration);
        var nextIcon = activeIcon + 1 == iconCount ? 0 : activeIcon + 1;

        var isPrimary = idx == activeIcon;
        var isSecondary = idx == nextIcon;

        if (iconTime < ShownDuration) // In shown state
        {
            if (isPrimary)
                return (true, false, 255);
            else if (isSecondary)
                return (false, true, 0);
        }
        else
        {
            // Time spent in the swap state 0->1
            var swapTime = (float)(iconTime - ShownDuration) / SwapDuration;
            if (isPrimary) // Fade out
                return (true, false, (byte)((1 - swapTime) * 255));
            else if (isSecondary) // Fade in
                return (false, true, (byte)(swapTime * 255));
        }

        return (false, false, 0);
    }

    protected AtkItemIcon(AtkResNode* node)
    {
        Node = node;
    }

    public static AtkItemIcon Create(AtkComponentIcon* node) =>
        new AtkComponentIconItemIcon(node);

    public static AtkItemIcon Create(AtkComponentButton* node) =>
        new AtkComponentButtonItemIcon(node);

    public static AtkItemIcon Create(AtkComponentListItemRenderer* node) =>
        new AtkComponentListItemRendererItemIcon(node);
}
