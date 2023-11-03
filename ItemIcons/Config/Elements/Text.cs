using ImGuiNET;
using System.Collections.Generic;

namespace ItemIcons.Config.Elements;

public sealed class Text : ConfigElement
{
    public string Data { get; }

    public Text(string text)
    {
        Data = text;
    }

    public override void Draw(Stack<object?> parents)
    {
        ImGui.TextUnformatted(Data);
    }
}
