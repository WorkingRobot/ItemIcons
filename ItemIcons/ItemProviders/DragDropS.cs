using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using AtkDragDropManager = ItemIcons.Agents.AtkDragDropManager;

namespace ItemIcons.ItemProviders;

internal static unsafe class DragDropS
{
    public static string AddonName => "DragDropS";

    public static AtkItemIcon? GetIcon()
    {
        var addon = Service.GameGui.GetAddonByName(AddonName);
        if (addon == nint.Zero)
            return null;
        
        return BaseItemProvider.TryGetDragDropIcon(addon, 2);
    }

    public static AtkItemIcon? GetDraggedIcon()
    {
        var m = (AtkDragDropManager*)&AtkStage.GetSingleton()->DragDropManager;
        if (m->UnkDragDrop_1 != null)
        {
            var node = (AtkComponentDragDrop*)m->UnkDragDrop_1->GetComponent();
            if (node != null && node->AtkComponentIcon != null)
                return AtkItemIcon.Create(node->AtkComponentIcon);
        }
        if (m->UnkDragDrop_2 != null)
        {
            var node = (AtkComponentDragDrop*)m->UnkDragDrop_2->GetComponent();
            if (node != null && node->AtkComponentIcon != null)
                return AtkItemIcon.Create(node->AtkComponentIcon);
        }
        return null;
    }
}
