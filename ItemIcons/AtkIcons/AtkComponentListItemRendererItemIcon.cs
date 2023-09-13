using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemIcons.AtkIcons;

internal sealed unsafe class AtkComponentListItemRendererItemIcon : AtkComponentBaseItemIcon
{
    protected override uint AttachedSiblingNodeId => 5;

    public AtkComponentListItemRendererItemIcon(AtkComponentListItemRenderer* node) : base(&node->AtkComponentButton.AtkComponentBase)
    {
    }
}
