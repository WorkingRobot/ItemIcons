using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;

namespace ItemIcons.Agents;

[StructLayout(LayoutKind.Explicit, Size = 0x58)]
[Agent(AgentId.MiragePrismPrismItemDetail)]
public unsafe struct AgentMiragePrismPrismItemDetail
{
    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    [FieldOffset(0x54)]
    public uint ItemId;
}
