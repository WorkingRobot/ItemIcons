using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Runtime.InteropServices;

namespace ItemIcons.Agents;

[StructLayout(LayoutKind.Explicit, Size = 0x30)]
public unsafe struct ItemOrderModuleSorterRetainerEntry
{
    [FieldOffset(0x00)] public ItemOrderModuleSorterRetainerEntry* A;
    [FieldOffset(0x08)] public ItemOrderModuleSorterRetainerEntry* B;
    [FieldOffset(0x10)] public ItemOrderModuleSorterRetainerEntry* Next;
    [FieldOffset(0x19)] public bool IsValid;
    [FieldOffset(0x20)] public ulong RetainerId;
    [FieldOffset(0x28)] public ItemOrderModuleSorter* Sorter;
}
