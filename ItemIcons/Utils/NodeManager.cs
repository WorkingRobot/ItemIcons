using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemIcons.Utils;

internal static unsafe class NodeManager
{
    public static AtkImageNode* CreateImageNode(uint nodeId, bool createAssets)
    {
        var imageNode = Construct<AtkImageNode>();
        imageNode->AtkResNode.Type = NodeType.Image;
        imageNode->AtkResNode.NodeId = nodeId;
        imageNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;
        imageNode->AtkResNode.DrawFlags = 0;
        imageNode->WrapMode = 1;
        imageNode->Flags = 0;

        if (createAssets)
        {
            var asset = Malloc<AtkUldAsset>();
            asset->Id = 0;
            asset->AtkTexture.Ctor();
            asset->AtkTexture.LoadIconTexture(0);

            var part = Malloc<AtkUldPart>();
            *part = new()
            {
                UldAsset = asset,
                U = 0,
                V = 0,
                Width = 0,
                Height = 0
            };

            var partsList = Malloc<AtkUldPartsList>();
            *partsList = new()
            {
                Id = 0,
                PartCount = 1,
                Parts = part
            };
            imageNode->PartsList = partsList;
        }
        else
            imageNode->PartsList = null;


        imageNode->AtkResNode.ToggleVisibility(false);

        return imageNode;
    }

    public static AtkTextNode* CreateTextNode(uint nodeId)
    {
        var textNode = Construct<AtkTextNode>();
        textNode->AtkResNode.Type = NodeType.Text;
        textNode->AtkResNode.NodeId = nodeId;
        textNode->AtkResNode.NodeFlags = NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.EmitsEvents;
        textNode->AtkResNode.DrawFlags = 0;

        textNode->AtkResNode.ToggleVisibility(false);

        return textNode;
    }

    public static void DestroyImageNode(AtkImageNode* image, AtkUldManager* uldManager, bool freeAssets)
    {
        if (image->AtkResNode.PrevSiblingNode != null)
            image->AtkResNode.PrevSiblingNode->NextSiblingNode = image->AtkResNode.NextSiblingNode;

        if (image->AtkResNode.NextSiblingNode != null)
            image->AtkResNode.NextSiblingNode->PrevSiblingNode = image->AtkResNode.PrevSiblingNode;

        uldManager->UpdateDrawNodeList();

        if (freeAssets)
        {
            for (var i = 0; i < image->PartsList->PartCount; i++)
            {
                var part = &image->PartsList->Parts[i];
                part->UldAsset->AtkTexture.ReleaseTexture();
                IMemorySpace.Free(part->UldAsset);
                IMemorySpace.Free(part);
            }
            IMemorySpace.Free(image->PartsList);
        }

        image->AtkResNode.Destroy(false);
        IMemorySpace.Free(image);
    }

    public static void DestroyTextNode(AtkTextNode* text, AtkUldManager* uldManager)
    {
        if (text->AtkResNode.PrevSiblingNode != null)
            text->AtkResNode.PrevSiblingNode->NextSiblingNode = text->AtkResNode.NextSiblingNode;

        if (text->AtkResNode.NextSiblingNode != null)
            text->AtkResNode.NextSiblingNode->PrevSiblingNode = text->AtkResNode.PrevSiblingNode;

        uldManager->UpdateDrawNodeList();

        text->AtkResNode.Destroy(false);
        IMemorySpace.Free(text);
    }


    public static AtkImageNode* GetImageNode(AtkComponentBase* component, uint nodeId)
    {
        var customNode = NodeUtils.GetNodeByIdStrict(component, nodeId);
        return customNode != null ? customNode->GetAsAtkImageNode() : (AtkImageNode*)null;
    }

    public static AtkTextNode* GetTextNode(AtkComponentBase* component, uint nodeId)
    {
        var customNode = NodeUtils.GetNodeByIdStrict(component, nodeId);
        return customNode != null ? customNode->GetAsAtkTextNode() : (AtkTextNode*)null;
    }

    public static void AppendNodeTo(AtkComponentNode* parent, AtkResNode* node, AtkResNode* siblingNode)
    {
        node->ParentNode = &parent->AtkResNode;

        node->PrevSiblingNode =
            siblingNode->PrevSiblingNode != null ?
            siblingNode->PrevSiblingNode : (AtkResNode*)null;
        siblingNode->PrevSiblingNode = node;
        node->NextSiblingNode = siblingNode;

        parent->Component->UldManager.UpdateDrawNodeList();
    }

    public static void AppendNodeToPrev(AtkComponentNode* parent, AtkResNode* node, uint siblingNodeId) =>
        AppendNodeTo(parent, node, NodeUtils.GetNodeById(parent->Component, siblingNodeId));

    public static T* Malloc<T>() where T : unmanaged =>
        (T*)IMemorySpace.GetUISpace()->Malloc((ulong)sizeof(T), 8);

    public static T* Construct<T>() where T : unmanaged, ICreatable =>
        IMemorySpace.GetUISpace()->Create<T>();
}
