using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System;
using System.Numerics;

namespace ItemIcons.IconTypes;

internal sealed record TextIcon : BaseIcon
{
    public required SeString Text { get; init; }
    public byte FontSize { get; init; } = 12;
    public byte LineSpacing { get; init; } = 14;
    public FontType FontType { get; init; } = FontType.Axis;
    public Vector4 TextColor { get; init; } = Vector4.One;
    public Vector4 EdgeColor { get; init; } = new(0, 0, 0, 1);
    public Vector4 BackgroundColor { get; init; } = Vector4.Zero;
    public AlignmentType Alignment { get; init; } = AlignmentType.TopLeft;
    public TextFlags Flags { get; init; }
    public TextFlags2 Flags2 { get; init; }

    private static ByteColor GetByteColor(Vector4 color)
    {
        color.X = Math.Clamp(color.X, 0, 1);
        color.Y = Math.Clamp(color.Y, 0, 1);
        color.Z = Math.Clamp(color.Z, 0, 1);
        color.W = Math.Clamp(color.W, 0, 1);

        return new ByteColor()
        {
            R = (byte)(int)(color.X * 255f + .5f),
            G = (byte)(int)(color.Y * 255f + .5f),
            B = (byte)(int)(color.Z * 255f + .5f),
            A = (byte)(int)(color.W * 255f + .5f),
        };
    }

    private unsafe bool ApplyText(AtkTextNode* node)
    {
        if (node->TextId != IconId)
        {
            node->TextId = IconId;
            node->SetText(Text.Encode());
            node->SetAlignment(Alignment);
            node->SetFont(FontType);
            node->FontSize = FontSize;
            node->LineSpacing = LineSpacing;
            node->TextColor = GetByteColor(TextColor);
            node->EdgeColor = GetByteColor(EdgeColor);
            node->BackgroundColor = GetByteColor(BackgroundColor);
            node->TextFlags = (byte)Flags;
            node->TextFlags2 = (byte)Flags2;
            node->ResizeNodeForCurrentText();
            return true;
        }
        return false;
    }

    public override unsafe void Apply(AtkItemIcon icon, bool usePrimary, byte alpha)
    {
        var node = usePrimary ? icon.TextNode1 : icon.TextNode2;

        node->AtkResNode.Color.A = alpha;

        if (ApplyText(node))
        {
            node->AtkResNode.X = (short)(BaseOffset + Offset);
            node->AtkResNode.Y = (short)(BaseOffset + Offset);

            node->AtkResNode.AddRed = (short)AddRGB.X;
            node->AtkResNode.AddGreen = (short)AddRGB.Y;
            node->AtkResNode.AddBlue = (short)AddRGB.Z;
            node->AtkResNode.MultiplyRed = (byte)MultiplyRGB.X;
            node->AtkResNode.MultiplyGreen = (byte)MultiplyRGB.Y;
            node->AtkResNode.MultiplyBlue = (byte)MultiplyRGB.Z;
        }

        NodeUtils.SetVisibility(&node->AtkResNode, true);
        icon.UpdateDirtyNode(&node->AtkResNode);
    }

    public bool Equals(TextIcon? icon)
    {
        if (icon is null)
            return false;

        return base.Equals(icon) &&
            Text == icon.Text &&
            FontSize == icon.FontSize &&
            LineSpacing == icon.LineSpacing &&
            FontType == icon.FontType &&
            TextColor == icon.TextColor &&
            EdgeColor == icon.EdgeColor &&
            BackgroundColor == icon.BackgroundColor &&
            Alignment == icon.Alignment &&
            Flags == icon.Flags &&
            Flags2 == icon.Flags2;
    }

    public override int GetHashCode() =>
        HashCode.Combine(HashCode.Combine(base.GetHashCode(), Text, FontSize, LineSpacing, FontType, TextColor), EdgeColor, BackgroundColor, Alignment, Flags, Flags2);
}
