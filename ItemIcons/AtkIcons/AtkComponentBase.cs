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

        if (TextNode == null)
        {
            TextNode = NodeManager.GetTextNode(Component, TextNodeId);
            if (TextNode == null)
            {
                TextNode = NodeManager.CreateTextNode(TextNodeId);
                NodeManager.AppendNodeTo(Component->OwnerNode, &TextNode->AtkResNode, &ImageNode2->AtkResNode);
            }
        }
    }

    public override void Destroy()
    {
        if (TextNode != null)
            NodeManager.DestroyTextNode(TextNode, &Component->UldManager);
        if (ImageNode2 != null)
            NodeManager.DestroyImageNode(ImageNode2, &Component->UldManager, false);
        if (ImageNode1 != null)
            NodeManager.DestroyImageNode(ImageNode1, &Component->UldManager, false);
    }

    public override unsafe void UpdateDirtyNode(AtkResNode* node)
    {
        base.UpdateDirtyNode(node);
        Service.Plugin.atkUldManagerUpdateNodeColors(&Component->UldManager, node, Node);
        Service.Plugin.atkUldManagerUpdateNodeTransform(&Component->UldManager, node, Node);
    }
}
