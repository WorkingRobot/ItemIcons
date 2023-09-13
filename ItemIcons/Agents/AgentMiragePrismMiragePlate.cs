using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop.Attributes;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;

namespace ItemIcons.Agents;

[StructLayout(LayoutKind.Explicit, Size = 776)]
[Agent(AgentId.MiragePrismMiragePlate)]
public unsafe struct AgentMiragePrismMiragePlate
{
    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    [FieldOffset(328)]
    [FixedSizeArray<MiragePlateItem>(14)]
    public unsafe fixed byte PlateItems[448];

    public unsafe Span<MiragePlateItem> PlateItemsSpan => new(Unsafe.AsPointer(ref PlateItems[0]), 14);
}

[StructLayout(LayoutKind.Explicit, Size = 0x20)]
public struct MiragePlateItem
{
    public enum SlotType : byte
    {
        Mainhand,
        Offhand,
        Head,
        Body,
        Hands,
        Waist,
        Legs,
        Feet,
        Ears,
        Neck,
        Wrist,
        RingRight,
        RingLeft,
        Empty = 0x0E
    }
    [FieldOffset(0x00)] public SlotType Slot;
    [FieldOffset(0x01)] public byte EquipSlotCategory;
    // [FieldOffset(0x02)] public byte EquipSlotCategory2;
    [FieldOffset(0x03)] public byte Stain;
    // [FieldOffset(0x04)] public byte Stain2;
    [FieldOffset(0x08)] public uint ItemId;
    [FieldOffset(0x10)] public ulong ModelMain;
    [FieldOffset(0x18)] public ulong ModelSub;
}
