using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;

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
        var m = &AtkStage.GetSingleton()->DragDropManager;
        if (m->DragDrop1 != null)
        {
            var node = (AtkComponentDragDrop*)m->DragDrop1->GetComponent();
            if (node != null && node->AtkComponentIcon != null)
                return AtkItemIcon.Create(node->AtkComponentIcon);
        }
        if (m->DragDrop2 != null)
        {
            var node = (AtkComponentDragDrop*)m->DragDrop2->GetComponent();
            if (node != null && node->AtkComponentIcon != null)
                return AtkItemIcon.Create(node->AtkComponentIcon);
        }
        return null;
    }
}
