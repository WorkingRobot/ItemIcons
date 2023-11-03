using System.Collections.Generic;

namespace ItemIcons.Config;

public class ConfigDrawState<T>
{
    public string Name { get; }
    public string? Description { get; }

    private ConfigAttribute Attribute { get; }

    public ConfigDrawState(string name, string? description = null)
    {
        Name = name;
        Description = description;

        Attribute = new(Name, Description);
    }

    internal void InjectValueAttribute(ConfigValueAttribute attribute)
    {
        Attribute.VerifierInstance = attribute.VerifierInstance;
        Attribute.DrawerInstance = attribute.DrawerInstance;
    }

    public bool Draw(ref T value, Stack<object?> parents) =>
        Attribute.Draw(ref value, parents);
}
