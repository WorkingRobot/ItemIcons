using ImGuiNET;
using System.Collections.Generic;

namespace ItemIcons.Config.Drawers;

internal sealed class BoolDrawer : ConfigDrawer<bool>
{
    public override bool Draw(ref bool value, ConfigAttribute config, Stack<object?> parents)
    {
        var ret = false;
        if (ImGui.Checkbox(config.Name, ref value))
            ret = true;
        if (config.Description != null)
        {
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(config.Description);
        }
        return ret;
    }
}
