using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ItemIcons.Config.Drawers;

public class ListedEnumDrawer<K, V> : ListedDrawer<K, V> where K : struct, Enum
{
    public override IReadOnlyList<(K Key, ConfigDrawState<V> State)> Keys => KeyArray;

    private static (K Key, ConfigDrawState<V> State)[] KeyArray { get; }

    static ListedEnumDrawer()
    {
        KeyArray = GetKeys().ToArray();
    }

    internal static IOrderedEnumerable<(K, ConfigDrawState<V>)> GetKeys()
    {
        var methods = GetExtensionMethods(typeof(K).Assembly, typeof(K));
        var method = methods.FirstOrDefault(m =>
            (
                m.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Equals("GetName", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Equals("ToName", StringComparison.OrdinalIgnoreCase)
            ) &&
            m.GetParameters().Length == 1 &&
            m.ReturnType.IsAssignableTo(typeof(string)));
        Func<K, string> getName =
            method == null ?
            (k => k.ToString()) :
            (k =>
                method.Invoke(null, new object?[] { k }) as string ??
                throw new ArgumentException($"Failed to get name of {typeof(K)} for {k}"));
        return Enum.GetValues<K>()
            .Select(k => (k, new ConfigDrawState<V>(getName(k))))
            .OrderBy(k => k.Item2.Name);
    }

    // https://stackoverflow.com/a/299526
    private static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType) =>
        assembly.GetTypes()
            .Where(type => type.IsSealed && !type.IsGenericType && !type.IsNested)
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
            .Where(method => method.GetParameters()[0].ParameterType == extendedType);
}
