using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;

namespace ItemIcons.Agents;

[StructLayout(LayoutKind.Explicit, Size = 336)]
[Agent(AgentId.ItemDetail)]
public unsafe struct AgentItemDetail
{
    [FieldOffset(0)]
    public AgentInterface AgentInterface;

    [FieldOffset(312)]
    public uint ItemId;
}
