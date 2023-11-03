using ItemIcons.AtkIcons;
using System;
using System.Numerics;

namespace ItemIcons.IconTypes;

public abstract record BaseIcon : IDisposable, IEquatable<BaseIcon>
{
    protected const short BaseOffset = -2;

    public virtual float Scale { get; init; } = 1f;
    public short Offset { get; init; }
    public Vector3 AddRGB { get; init; }
    public Vector3 MultiplyRGB { get; init; } = new(100);

    public virtual uint IconId { get; set; }

    public abstract void Apply(AtkItemIcon icon, bool usePrimary, byte alpha);

    public virtual void Dispose() { }

    public virtual bool Equals(BaseIcon? other)
    {
        if (other is null)
            return false;

        return Scale == other.Scale &&
            Offset == other.Offset &&
            AddRGB == other.AddRGB &&
            MultiplyRGB == other.MultiplyRGB;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Scale, Offset, AddRGB, MultiplyRGB);
}
