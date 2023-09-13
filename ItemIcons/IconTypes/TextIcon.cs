using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using System;
using System.Numerics;

namespace ItemIcons.IconTypes;

internal sealed record TextIcon : BaseIcon
{
    public required string Text { get; init; }
    public required byte FontSize { get; init; } = 12;
    public required Vector4 TextColor { get; init; } = Vector4.One;
    public required Vector4 EdgeColor { get; init; } = new(0, 0, 0, 1);
    public required Vector4 BackgroundColor { get; init; } = Vector4.One;
    public required AlignmentType Alignment { get; init; } = AlignmentType.TopLeft;
    public required TextFlags Flags { get; init; } = TextFlags.AutoAdjustNodeSize | TextFlags.Bold | TextFlags.Edge;

    public override void Apply(AtkItemIcon icon, bool isPrimary, byte alpha)
    {
        throw new NotImplementedException();
    }
}
