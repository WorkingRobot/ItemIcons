using System;
using System.Collections.Generic;

namespace ItemIcons.Config;

public abstract class ConfigVerifier
{
    public abstract bool CanVerify(Type type);

    public abstract bool Verify(object? value);

    public abstract bool IsDisabled(object? value, Stack<object?> parents);

    public static ConfigVerifier Default { get; } = new DefaultVerifier();
}

public abstract class ConfigVerifier<T> : ConfigVerifier
{
    public sealed override bool CanVerify(Type type) =>
        type.IsAssignableFrom(type);

    public sealed override bool Verify(object? value) =>
        value is T t && Verify(t);

    public sealed override bool IsDisabled(object? value, Stack<object?> parents) =>
        value is T t && IsDisabled(t, parents);

    public abstract bool Verify(T value);

    public abstract bool IsDisabled(T value, object? parent);
}

public class DefaultVerifier : ConfigVerifier
{
    public override bool CanVerify(Type type) => true;

    public override bool Verify(object? value) => true;

    public override bool IsDisabled(object? value, Stack<object?> parents) => false;
}
