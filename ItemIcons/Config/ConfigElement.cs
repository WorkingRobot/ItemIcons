using System.Collections.Generic;

namespace ItemIcons.Config;

public abstract class ConfigElement
{
    public abstract void Draw(Stack<object?> parents);
}
