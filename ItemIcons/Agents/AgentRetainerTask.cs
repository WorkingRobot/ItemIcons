using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;

namespace ItemIcons.Agents;

[StructLayout(LayoutKind.Explicit, Size = 0x90)]
[Agent(AgentId.RetainerTask)]
public unsafe struct AgentRetainerTask
{
    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    // 01 - Request Assignment
    // 02 - Venture in Progress
    // 03 - Completed Venture
    [FieldOffset(0x28)]
    public uint DisplayType;

    [FieldOffset(0x30)]
    public void*** FuncPtrPtr;

    [FieldOffset(0x38)]
    public uint OpenerAddonId;

    [FieldOffset(0x3C)]
    public uint Unk3C;

    [FieldOffset(0x40)]
    public ushort RetainerTaskRow;

    [FieldOffset(0x44)]
    public uint RewardXP;

    [FieldOffset(0x4C)]
    public uint Unk4C;

    [FieldOffset(0x50)]
    public unsafe fixed uint RewardItemId[2];

    [FieldOffset(0x58)]
    public unsafe fixed uint RewardItemCount[2];

    [FieldOffset(0x60)]
    public ulong Unk60;

    [FieldOffset(0x68)]
    public bool Unk68;

    [FieldOffset(0x69)]
    public bool RetainerTaskRowFailed;

    [FieldOffset(0x6C)]
    public uint RetainerTaskLvRange;

    [FieldOffset(0x70)]
    public bool IsRandomTask;

    [FieldOffset(0x74)]
    public uint RetainerTaskId;

    [FieldOffset(0x78)]
    public uint CompareItemId;

    [FieldOffset(0x7C)]
    public uint CancelVentureYesNoAddonId;

    [FieldOffset(0x80)]
    public bool IsLoading;

    [FieldOffset(0x84)]
    public uint XPToReward;

    [FieldOffset(0x88)]
    public bool IsQuickExploration;
}
