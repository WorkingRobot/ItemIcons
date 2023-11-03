using System;
using System.Collections.Generic;

namespace ItemIcons.Config.Drawers;

internal sealed class ElementDrawer : ConfigDrawer
{
    public override bool CanDraw(Type type) =>
        type.IsAssignableTo(typeof(ConfigElement));

    public override bool Draw<T>(ref T value, ConfigAttribute config, Stack<object?> parents)
    {
        if (value is ConfigElement element)
            element.Draw(parents);

        return false;
    }
}
