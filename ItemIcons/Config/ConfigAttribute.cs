using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ItemIcons.Config;

[AttributeUsage(AttributeTargets.Property)]
public class ConfigAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; }
    public Type? Verifier { get; }
    public Type? Drawer { get; }

    internal Type? SelfType { get; set; }
    internal ConfigVerifier? VerifierInstance { get; set; }
    internal ConfigDrawer? DrawerInstance { get; set; }

    internal ConfigValueAttribute? ValueAttribute { get; set; }

    public ConfigAttribute(string name = "", string? description = null, Type? verifier = null, Type? drawer = null, object?[]? verifierArgs = null, object?[]? drawerArgs = null)
    {
        Name = name;
        Description = description;

        Verifier = verifier;
        if (Verifier is not null)
        {
            if (!Verifier.IsAssignableTo(typeof(ConfigVerifier)))
                throw new ArgumentException($"Verifier type {Verifier} must be a {typeof(ConfigVerifier)}");
            VerifierInstance = Activator.CreateInstance(Verifier, verifierArgs) as ConfigVerifier ??
                throw new ArgumentException($"Failed to create verifier of type {Verifier}");
        }

        Drawer = drawer;
        if (Drawer is not null)
        {
            if (!Drawer.IsAssignableTo(typeof(ConfigDrawer)))
                throw new ArgumentException($"Drawer type {Drawer} must be a {typeof(ConfigDrawer)}");
            DrawerInstance = Activator.CreateInstance(Drawer, drawerArgs) as ConfigDrawer ??
                throw new ArgumentException($"Failed to create drawer of type {Drawer}");
        }
    }

    private void InitializeInstances(Type propertyType)
    {
        if (SelfType == null)
            SelfType = propertyType;
        else if (SelfType != propertyType)
            throw new ArgumentException($"ConfigAttribute for {SelfType} cannot be used on type {propertyType}");

        if (DrawerInstance == null)
        {
            DrawerInstance ??= propertyType.GetCustomAttribute<ConfigClassAttribute>()?.DrawerInstance;

            DrawerInstance ??= ConfigDrawer.GetDrawer(propertyType) ??
                throw new ArgumentException($"No drawer found for type {propertyType}");
        }
        if (VerifierInstance == null)
        {
            VerifierInstance ??= propertyType.GetCustomAttribute<ConfigClassAttribute>()?.VerifierInstance;

            VerifierInstance ??= ConfigVerifier.Default;
        }
    }

    private void Initialize(Type propertyType)
    {
        InitializeInstances(propertyType);

        if (!DrawerInstance!.CanDraw(propertyType))
            throw new ArgumentException($"Drawer {DrawerInstance.GetType()} cannot draw type {propertyType}");

        if (!VerifierInstance!.CanVerify(propertyType))
            throw new ArgumentException($"Verifier {VerifierInstance.GetType()} cannot verify type {propertyType}");
    }

    internal void InitializeProperty(PropertyInfo property)
    {
        Initialize(property.PropertyType);

        ValueAttribute ??= property.GetCustomAttribute<ConfigValueAttribute>();
    }

    internal bool Draw<T>(ref T value, Stack<object?> parents)
    {
        Initialize(typeof(T));

        var oldValue = value;
        using var _ = ImRaii.Disabled(VerifierInstance!.IsDisabled(value, parents));
        var ret = DrawerInstance!.Draw(ref value, this, parents);
        if (ret && !VerifierInstance.Verify(value))
            value = oldValue;
        return ret;
    }

    private MethodInfo? DrawMethod { get; set; }
    internal bool DrawProperty<T>(ref T item, PropertyInfo property, Stack<object?> parents)
    {
        var propValue = property.GetValue(item);
        DrawMethod ??= GetType()
                .GetMethod(nameof(Draw), BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(property.PropertyType);
        var args = new object[] { propValue!, parents };
        var ret = (bool)DrawMethod.Invoke(this, args)!;
        if (ret)
            property.SetValue(item, args[0]);
        return ret;
    }
}
