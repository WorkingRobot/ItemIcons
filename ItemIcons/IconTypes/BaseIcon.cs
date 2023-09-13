using ItemIcons.AtkIcons;
using System;
using System.Numerics;

namespace ItemIcons.IconTypes;

public abstract record BaseIcon : IDisposable
{
    protected const short BaseOffset = -2;

    public virtual float Scale { get; init; } = 1f;
    public short Offset { get; init; }
    public Vector3 AddRGB { get; init; }
    public Vector3 MultiplyRGB { get; init; } = new(100);

    public virtual uint IconId { set { } }

    public abstract void Apply(AtkItemIcon icon, bool usePrimary, byte alpha);

    public virtual void Dispose() { }
}
