using System;

namespace ItemIcons.Config;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ConfigClassAttribute : Attribute
{
    public Type Verifier { get; }
    public Type Drawer { get; set; }

    internal ConfigVerifier? VerifierInstance { get; }
    internal ConfigDrawer? DrawerInstance { get; }

    public ConfigClassAttribute(Type? verifier = null, Type? drawer = null, object?[]? verifierArgs = null, object?[]? drawerArgs = null)
    {
        if (verifier is null)
        {
            VerifierInstance = ConfigVerifier.Default;
            Verifier = VerifierInstance.GetType();
        }
        else
        {
            Verifier = verifier;

            if (!Verifier.IsAssignableTo(typeof(ConfigVerifier)))
                throw new ArgumentException($"Verifier type {Verifier} must be a {typeof(ConfigVerifier)}");
            VerifierInstance = Activator.CreateInstance(Verifier, verifierArgs) as ConfigVerifier ??
                throw new ArgumentException($"Failed to create verifier of type {Verifier}");
        }

        if (drawer is null)
        {
            DrawerInstance = ConfigDrawer.ClassDefault;
            Drawer = DrawerInstance.GetType();
        }
        else
        {
            Drawer = drawer;

            if (!Drawer.IsAssignableTo(typeof(ConfigDrawer)))
                throw new ArgumentException($"Drawer type {Drawer} must be a {typeof(ConfigDrawer)}");
            DrawerInstance = Activator.CreateInstance(Drawer, drawerArgs) as ConfigDrawer ??
                throw new ArgumentException($"Failed to create drawer of type {Drawer}");
        }
    }
}
