using ItemIcons.Config.Drawers;
using System;

namespace ItemIcons.Config;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigValueAttribute : Attribute
{
    public Type? Verifier { get; }
    public Type? Drawer { get; set; }
    public Type? Constructor { get; set; }

    internal ConfigVerifier? VerifierInstance { get; }
    internal ConfigDrawer? DrawerInstance { get; }
    internal INewConstructor? ConstructorInstance { get; }

    public ConfigValueAttribute(Type? verifier = null, Type? drawer = null, object?[]? verifierArgs = null, object?[]? drawerArgs = null, Type? constructor = null, object?[]? constructorArgs = null)
    {
        if (verifier is not null)
        {
            Verifier = verifier;

            if (!Verifier.IsAssignableTo(typeof(ConfigVerifier)))
                throw new ArgumentException($"Verifier type {Verifier} must be a {typeof(ConfigVerifier)}");
            VerifierInstance = Activator.CreateInstance(Verifier, verifierArgs) as ConfigVerifier ??
                throw new ArgumentException($"Failed to create verifier of type {Verifier}");
        }

        if (drawer is not null)
        {
            Drawer = drawer;

            if (!Drawer.IsAssignableTo(typeof(ConfigDrawer)))
                throw new ArgumentException($"Drawer type {Drawer} must be a {typeof(ConfigDrawer)}");
            DrawerInstance = Activator.CreateInstance(Drawer, drawerArgs) as ConfigDrawer ??
                throw new ArgumentException($"Failed to create drawer of type {Drawer}");
        }

        if (constructor is not null)
        {
            Constructor = constructor;

            if (!Constructor.IsAssignableTo(typeof(INewConstructor)))
                throw new ArgumentException($"Constructor type {Constructor} must be a {typeof(INewConstructor)}");
            ConstructorInstance = Activator.CreateInstance(Constructor, constructorArgs) as INewConstructor ??
                throw new ArgumentException($"Failed to create constructor of type {Constructor}");
        }
    }
}
