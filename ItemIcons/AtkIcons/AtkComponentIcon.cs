using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemIcons.AtkIcons;

internal sealed unsafe class AtkComponentIconItemIcon : AtkComponentBaseItemIcon
{
    protected override uint AttachedSiblingNodeId => 2;

    public AtkComponentIconItemIcon(AtkComponentIcon* node) : base(&node->AtkComponentBase)
    {
    }
}
