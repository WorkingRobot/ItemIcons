using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemIcons.AtkIcons;

internal sealed unsafe class AtkComponentButtonItemIcon : AtkComponentBaseItemIcon
{
    protected override uint AttachedSiblingNodeId => 2;

    public AtkComponentButtonItemIcon(AtkComponentButton* node) : base(&node->AtkComponentBase)
    {
    }
}
