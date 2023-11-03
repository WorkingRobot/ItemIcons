using ImGuiNET;
using System.Collections.Generic;

namespace ItemIcons.Config.Elements;

public sealed class Separator : ConfigElement
{
    public override void Draw(Stack<object?> parents)
    {
        ImGui.Separator();
    }
}
