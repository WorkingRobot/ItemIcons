using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.Utils;

namespace ItemIcons.AtkIcons;

internal abstract unsafe class AtkComponentBaseItemIcon : AtkItemIcon
{
    protected AtkComponentBase* Component { get; }

    protected abstract uint AttachedSiblingNodeId { get; }

    public AtkComponentBaseItemIcon(AtkComponentBase* node) : base(&node->OwnerNode->AtkResNode)
    {
        Component = node;
    }

    public override void Setup()
    {
        if (ImageNode1 == null)
        {
            ImageNode1 = NodeManager.GetImageNode(Component, ImageNode1Id);
            if (ImageNode1 == null)
            {
                ImageNode1 = NodeManager.CreateImageNode(ImageNode1Id, false);
                NodeManager.AppendNodeToPrev(Component->OwnerNode, &ImageNode1->AtkResNode, AttachedSiblingNodeId);
            }
        }

        if (ImageNode2 == null)
        {
            ImageNode2 = NodeManager.GetImageNode(Component, ImageNode2Id);
            if (ImageNode2 == null)
            {
                ImageNode2 = NodeManager.CreateImageNode(ImageNode2Id, false);
                NodeManager.AppendNodeTo(Component->OwnerNode, &ImageNode2->AtkResNode, &ImageNode1->AtkResNode);
            }
        }

        if (TextNode1 == null)
        {
            TextNode1 = NodeManager.GetTextNode(Component, TextNode1Id);
            if (TextNode1 == null)
            {
                TextNode1 = NodeManager.CreateTextNode(TextNode1Id);
                NodeManager.AppendNodeTo(Component->OwnerNode, &TextNode1->AtkResNode, &ImageNode2->AtkResNode);
            }
        }

        if (TextNode2 == null)
        {
            TextNode2 = NodeManager.GetTextNode(Component, TextNode2Id);
            if (TextNode2 == null)
            {
                TextNode2 = NodeManager.CreateTextNode(TextNode2Id);
                NodeManager.AppendNodeTo(Component->OwnerNode, &TextNode2->AtkResNode, &TextNode1->AtkResNode);
            }
        }
    }

    public override void Destroy()
    {
        if (TextNode1 != null)
            NodeManager.DestroyTextNode(TextNode1, &Component->UldManager);
        if (TextNode2 != null)
            NodeManager.DestroyTextNode(TextNode2, &Component->UldManager);
        if (ImageNode2 != null)
            NodeManager.DestroyImageNode(ImageNode2, &Component->UldManager, false);
        if (ImageNode1 != null)
            NodeManager.DestroyImageNode(ImageNode1, &Component->UldManager, false);
    }

    public override unsafe void UpdateDirtyNode(AtkResNode* node)
    {
        base.UpdateDirtyNode(node);
        Service.Plugin.atkUldManagerUpdateNodeTree(&Component->UldManager, node, node->ParentNode, true);
    }
}
