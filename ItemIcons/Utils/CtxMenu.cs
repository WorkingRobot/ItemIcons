using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CSAgentContext = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentContext;
using CSAgentInventoryContext = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInventoryContext;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace ItemIcons.Utils;

public sealed unsafe class MenuItemClickedArgs
{
    public nint AgentPtr { get; }
    private bool? IsDefaultAgentContext { get; }

    public InventoryItem Item => *InventoryContext->TargetInventorySlot;

    private Action<SeString?, IReadOnlyList<MenuItem>> OnOpenSubmenu { get; }

    internal MenuItemClickedArgs(Action<SeString?, IReadOnlyList<MenuItem>> openSubmenu, AgentInterface* agent, bool? isDefaultAgentContext)
    {
        OnOpenSubmenu = openSubmenu;
        AgentPtr = (nint)agent;
        IsDefaultAgentContext = isDefaultAgentContext;
    }

    public void OpenSubmenu(SeString name, IReadOnlyList<MenuItem> items) =>
        OnOpenSubmenu(name, items);

    public void OpenSubmenu(IReadOnlyList<MenuItem> items) =>
        OnOpenSubmenu(null, items);

    private AgentInventoryContext* InventoryContext =>
        IsDefaultAgentContext == false ?
            (AgentInventoryContext*)AgentPtr :
            throw new InvalidOperationException("Not an inventory menu");
}

public sealed unsafe class MenuOpenedArgs
{
    public nint AgentPtr { get; }
    private bool? IsDefaultAgentContext { get; }

    public InventoryItem Item => *InventoryContext->TargetInventorySlot;

    private Action<MenuItem> OnAddMenuItem { get; }

    internal MenuOpenedArgs(Action<MenuItem> addMenuItem, AgentInterface* agent, bool? isDefaultAgentContext)
    {
        OnAddMenuItem = addMenuItem;
        AgentPtr = (nint)agent;
        IsDefaultAgentContext = isDefaultAgentContext;
    }

    public void AddMenuItem(MenuItem item) =>
        OnAddMenuItem(item);

    private AgentInventoryContext* InventoryContext =>
        IsDefaultAgentContext == false ?
            (AgentInventoryContext*)AgentPtr :
            throw new InvalidOperationException("Not an inventory menu");
}

public sealed record MenuItem
{
    public required SeString Name { get; init; }
    public required Action<MenuItemClickedArgs> OnClicked { get; init; }
    public int Priority { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool IsSubmenu { get; init; }
    public bool IsReturn { get; init; }
}

internal unsafe sealed class CtxMenu : IDisposable
{
    [Signature("E8 ?? ?? ?? ?? 0F B7 C0 48 83 C4 60", DetourName = nameof(ContextAddonOpenByAgentDetour))]
    private readonly Hook<ContextAddonOpenByAgentDelegate> contextAddonOpenByAgentHook = null!;
    private unsafe delegate nint ContextAddonOpenByAgentDelegate(RaptureAtkModule* module, byte* addonName, AtkUnitBase* addon, int valueCount, AtkValue* values, AgentInterface* agent, nint a7, ushort a8);

    // Called when addon is clicked
    [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 80 B9", DetourName = nameof(ContextMenuOnVf72Detour))]
    private readonly Hook<ContextMenuOnVf72Delegate> contextMenuOnVf72Hook = null!;
    private unsafe delegate bool ContextMenuOnVf72Delegate(AddonContextMenu* addon, int selectedIdx, byte a3);

    // Client::UI::RaptureAtkModule_OpenAddon
    [Signature("E8 ?? ?? ?? ?? 66 89 46 50")]
    private readonly RaptureAtkModuleOpenAddonDelegate raptureAtkModuleOpenAddon = null!;
    // Returns new addon id
    private unsafe delegate nint RaptureAtkModuleOpenAddonDelegate(RaptureAtkModule* a1, uint addonNameId, uint valueCount, AtkValue* values, AgentInterface* parentAgent, long unk, ushort ownerAddonId, int unk2);

    private List<MenuItem> DefaultContextItems { get; } = new();
    private List<MenuItem> InventoryContextItems { get; } = new();

    private AgentInterface* SelectedAgent { get; set; }
    private bool? SelectedIsDefaultAgentContext { get; set; }
    private List<MenuItem>? SelectedItems { get; set; }

    // -1 -> -inf: native items
    // 0 -> inf: selected items
    private List<int> MenuCallbackIds { get; } = new();
    private IReadOnlyList<MenuItem>? SubmenuItems { get; set; }

    public event Action<MenuOpenedArgs>? OnMenuOpened;

    public CtxMenu()
    {
        Service.GameInteropProvider.InitializeFromAttributes(this);
        contextAddonOpenByAgentHook.Enable();
        contextMenuOnVf72Hook.Enable();
    }

    public static SeString GetPrefixedName(SeString name, char prefix = 'D', ushort colorKey = 539)
    {
        if (!char.IsAsciiLetterUpper(prefix))
            throw new ArgumentException("Prefix must be an uppercase letter", nameof(prefix));

        return new SeStringBuilder()
            .AddUiForeground($"{(SeIconChar.BoxedLetterA + prefix - 'A').ToIconString()} ", colorKey)
            .Append(name)
            .Build();
    }

    public void AddDefaultContextItem(MenuItem item)
    {
        DefaultContextItems.Add(item);
    }

    public void AddInventoryContextItem(MenuItem item)
    {
        InventoryContextItems.Add(item);
    }

    private AtkValue* ExpandContextMenuArray(Span<AtkValue> oldValues, int newSize)
    {
        // if the array has enough room, don't reallocate
        if (oldValues.Length >= newSize)
            return (AtkValue*)Unsafe.AsPointer(ref oldValues[0]);

        var size = (sizeof(AtkValue) * newSize) + 8;
        var newArray = (nint)IMemorySpace.GetUISpace()->Malloc((ulong)size, 0);
        NativeMemory.Fill((void*)newArray, (nuint)size, 0);

        *(ulong*)newArray = (ulong)newSize;

        // copy old memory if existing
        if (!oldValues.IsEmpty)
            oldValues.CopyTo(new((void*)(newArray + 8), oldValues.Length));
        // IMemorySpace.Free((void*)((nint)Unsafe.AsPointer(ref oldValues[0]) - 8), (ulong)((sizeof(AtkValue) * oldValues.Length) + 8));

        return (AtkValue*)(newArray + 8);
    }

    private AtkValue* CreateEmptySubmenuContextMenuArray(SeString name, int x, int y)
    {
        // 0: UInt = ContextItemCount
        // 1: String = Name
        // 2: Int = PositionX
        // 3: Int = PositionY
        // 4: Bool = false
        // 5: UInt = ContextItemSubmenuMask
        // 6: UInt = ReturnArrowMask (_gap_0x6BC ? 1 << (ContextItemCount - 1) : 0)
        // 7: UInt = 1

        var values = ExpandContextMenuArray(Span<AtkValue>.Empty, 8);
        values[0].ChangeType(ValueType.UInt);
        values[0].UInt = 0;
        values[1].ChangeType(ValueType.String);
        values[1].SetString(name.Encode().NullTerminate());
        values[2].ChangeType(ValueType.Int);
        values[2].Int = x;
        values[3].ChangeType(ValueType.Int);
        values[3].Int = y;
        values[4].ChangeType(ValueType.Bool);
        values[4].Byte = 0;
        values[5].ChangeType(ValueType.UInt);
        values[5].UInt = 0;
        values[6].ChangeType(ValueType.UInt);
        values[6].UInt = 0;
        values[7].ChangeType(ValueType.UInt);
        values[7].UInt = 1;
        return values;
    }

    private static string DebugValues(int valueCount, AtkValue* values)
    {
        var b = new StringBuilder();
        for (var i = 0; i < valueCount; ++i)
        {
            var v = values[i];
            var s = v.Type switch
            {
                ValueType.Int => $"{v.Int}",
                ValueType.Bool => $"{v.Byte != 0}",
                ValueType.UInt => $"{v.UInt}",
                ValueType.Float => $"{v.Float}",
                ValueType.String or ValueType.AllocatedString or ValueType.String8 => $"{(v.String != null ? MemoryHelper.ReadSeStringNullTerminated((nint)v.String) : "null")}",
                _ => $"unk ({v.Type})"
            };
            b.AppendLine($"[{i}] {s}");
            Log.Debug($"[{i}]({v.Type}) {s}");
        }
        return b.ToString();
    }

    private void SetupContextMenu(IReadOnlyList<MenuItem> items, AddonContextMenu* addon, ref int valueCount, ref AtkValue* values)
    {
        // 0: UInt = Item Count
        // 1: UInt = 0 (probably window name, just unused)
        // 2: UInt = Return Mask (?)
        // 3: UInt = Submenu Mask
        // 4: UInt = OpenAtCursorPosition ? 2 : 1
        // 5: UInt = 0
        // 6: UInt = 0

        MenuCallbackIds.Clear();

        const int offset = 7;

        var itemsWithIdx = items.Select((item, idx) => (item, idx)).Order();
        var prefixItems = itemsWithIdx.Where(i => i.item.Priority < 0).ToArray();
        var suffixItems = itemsWithIdx.Where(i => i.item.Priority >= 0).ToArray();

        var nativeMenuSize = (int)values[0].UInt;
        var prefixMenuSize = prefixItems.Length;
        var suffixMenuSize = suffixItems.Length;

        var hasGameDisabled = valueCount - offset - nativeMenuSize > 0;

        var hasCustomDisabled = items.Any(item => !item.IsEnabled);
        var hasAnyDisabled = hasGameDisabled || hasCustomDisabled;

        values = ExpandContextMenuArray(new(values, valueCount), (nativeMenuSize + items.Count) * (hasAnyDisabled ? 2 : 1) + offset);
        var offsetData = new Span<AtkValue>(values, offset);
        var nameData = new Span<AtkValue>(values + offset, nativeMenuSize + items.Count);
        var disabledData = hasAnyDisabled ? new Span<AtkValue>(values + offset + nativeMenuSize + items.Count, nativeMenuSize + items.Count) : Span<AtkValue>.Empty;

        var returnMask = offsetData[2].UInt;
        var submenuMask = offsetData[3].UInt;

        returnMask <<= prefixMenuSize;
        submenuMask <<= prefixMenuSize;

        if (!hasGameDisabled && hasCustomDisabled)
        {
            // re-add disabled args
            for (var i = 0; i < nativeMenuSize; ++i)
            {
                disabledData[i] = new();
                disabledData[i].ChangeType(ValueType.Int);
                disabledData[i].Int = 0;
            }
        }

        nameData[..nativeMenuSize].CopyTo(nameData.Slice(prefixMenuSize, nativeMenuSize));
        if (!disabledData.IsEmpty)
            disabledData[..nativeMenuSize].CopyTo(disabledData.Slice(prefixMenuSize, nativeMenuSize));

        for (var i = 0; i < prefixMenuSize; ++i)
        {
            var (item, idx) = prefixItems[i];
            MenuCallbackIds.Add(idx);

            if (hasAnyDisabled)
            {
                disabledData[i].ChangeType(ValueType.Int);
                disabledData[i].Int = item.IsEnabled ? 0 : 1;
            }

            if (item.IsReturn)
                returnMask |= 1u << i;
            if (item.IsSubmenu)
                submenuMask |= 1u << i;

            nameData[i].ChangeType(ValueType.String);
            nameData[i].SetString(item.Name.Encode().NullTerminate());
        }

        MenuCallbackIds.AddRange(Enumerable.Range(0, nativeMenuSize).Select(i => -i - 1));

        for (var i = prefixMenuSize + nativeMenuSize; i < prefixMenuSize + nativeMenuSize + suffixMenuSize; ++i)
        {
            var (item, idx) = suffixItems[i - prefixMenuSize - nativeMenuSize];
            MenuCallbackIds.Add(idx);

            if (hasAnyDisabled)
            {
                disabledData[i].ChangeType(ValueType.Int);
                disabledData[i].Int = item.IsEnabled ? 0 : 1;
            }

            if (item.IsReturn)
                returnMask |= 1u << i;
            if (item.IsSubmenu)
                submenuMask |= 1u << i;

            nameData[i].ChangeType(ValueType.String);
            nameData[i].SetString(item.Name.Encode().NullTerminate());
        }

        offsetData[2].UInt = returnMask;
        offsetData[3].UInt = submenuMask;

        offsetData[0].UInt += (uint)items.Count;

        valueCount = nativeMenuSize + items.Count;
        if (hasAnyDisabled)
            valueCount *= 2;
        valueCount += offset;
    }

    private void SetupContextSubMenu(IReadOnlyList<MenuItem> items, AddonContextMenuTitle* addon, ref int valueCount, ref AtkValue* values)
    {
        MenuCallbackIds.Clear();

        const int offset = 8;

        var sortedItems = items.Select((item, idx) => (item, idx)).OrderBy(i => i.item.Priority).ToArray();
        var hasDisabled = items.Any(item => !item.IsEnabled);

        values = ExpandContextMenuArray(new(values, offset), items.Count * (hasDisabled ? 2 : 1) + offset);

        var beginIdx = offset;
        var endIdx = beginIdx + items.Count;

        uint submenuMask = 0;
        uint returnMask = 0;

        for (var i = 0; i < items.Count; ++i)
        {
            var (item, idx) = sortedItems[i];
            MenuCallbackIds.Add(idx);

            if (hasDisabled)
            {
                values[i + endIdx] = new();
                values[i + endIdx].ChangeType(ValueType.Int);
                values[i + endIdx].Int = item.IsEnabled ? 0 : 1;
            }

            if (item.IsSubmenu)
                submenuMask |= 1u << i;
            if (item.IsReturn)
                returnMask |= 1u << i;

            values[i + offset].ChangeType(ValueType.String);
            values[i + offset].SetString(item.Name.Encode().NullTerminate());
        }

        values[0].UInt += (uint)items.Count;
        
        values[5].UInt = submenuMask;
        values[6].UInt = returnMask;

        valueCount = items.Count;
        if (hasDisabled)
            valueCount *= 2;
        valueCount += offset;
    }

    private nint ContextAddonOpenByAgentDetour(RaptureAtkModule* module, byte* addonName, AtkUnitBase* addon, int valueCount, AtkValue* values, AgentInterface* agent, nint a7, ushort a8)
    {
        var n = MemoryHelper.ReadStringNullTerminated((nint)addonName);
        if (n == "ContextMenu")
        {
            MenuCallbackIds.Clear();
            SelectedAgent = agent;
            if (SelectedAgent == CSAgentInventoryContext.Instance())
            {
                SelectedItems = InventoryContextItems;
                SelectedIsDefaultAgentContext = false;
            }
            else if (SelectedAgent == CSAgentContext.Instance())
            {
                SelectedItems = DefaultContextItems;
                SelectedIsDefaultAgentContext = true;
            }
            else
            {
                SelectedItems = null;
                SelectedIsDefaultAgentContext = null;
            }
            SubmenuItems = null;

            if (SelectedItems != null)
            {
                SelectedItems = new List<MenuItem>(SelectedItems);
                OnMenuOpened?.Invoke(new(SelectedItems.Add, SelectedAgent, SelectedIsDefaultAgentContext));
                SetupContextMenu(SelectedItems, (AddonContextMenu*)addon, ref valueCount, ref values);
            }
        }
        if (n == "AddonContextSub")
        {
            MenuCallbackIds.Clear();
            if (SubmenuItems != null)
                SetupContextSubMenu(SubmenuItems, (AddonContextMenuTitle*)addon, ref valueCount, ref values);
        }
        return contextAddonOpenByAgentHook.Original(module, addonName, addon, valueCount, values, agent, a7, a8);
    }

    private void OpenSubmenu(SeString name, IReadOnlyList<MenuItem> submenuItems, int posX, int posY)
    {
        if (submenuItems.Count == 0)
            throw new ArgumentException("Submenu must not be empty", nameof(submenuItems));

        SubmenuItems = submenuItems;

        var module = RaptureAtkModule.Instance();
        var paramCount = 8;
        var values = CreateEmptySubmenuContextMenuArray(name, posX, posY);
        var ownerAddonId = SelectedAgent->AddonId;
        // 0: UInt = ContextItemCount
        // 1: skipped?
        // 2: Int = PositionX
        // 3: Int = PositionY
        // 4: Bool = false
        // 5: UInt = ContextItemSubmenuMask
        // 6: UInt = _gap_0x6BC ? 1 << (ContextItemCount - 1) : 0
        // 7: UInt = 1
        if (SelectedIsDefaultAgentContext == true)
        {
            // 445 16; 10; 71 4
            raptureAtkModuleOpenAddon(module, 445, (uint)paramCount, values, SelectedAgent, 71, 16, 4);
        }
        else if (SelectedIsDefaultAgentContext == false)
        {
            // 445 77; 10; 0 4
            raptureAtkModuleOpenAddon(module, 445, (uint)paramCount, values, SelectedAgent, 0, 77, 4);
        }
        else
            Log.Debug($"Unknown agent {(nint)SelectedAgent:X8}");
    }

    private bool ContextMenuOnVf72Detour(AddonContextMenu* addon, int selectedIdx, byte a3)
    {
        var items = SubmenuItems ?? SelectedItems;
        if (items == null)
            goto original;
        if (MenuCallbackIds.Count == 0)
            goto original;
        if (selectedIdx < 0)
            goto original;
        if (selectedIdx >= MenuCallbackIds.Count)
            goto original;

        var callbackId = MenuCallbackIds[selectedIdx];

        if (callbackId < 0)
        {
            selectedIdx = -callbackId - 1;
            goto original;
        }
        else
        {
            var item = items[callbackId];
            var openedSubmenu = false;

            try
            {
                item.OnClicked(new(
                    (name, items) =>
                    {
                        short x, y;
                        addon->AtkUnitBase.GetPosition(&x, &y);
                        OpenSubmenu(name ?? item.Name, items, x, y);
                        openedSubmenu = true;
                    },
                    SelectedAgent,
                    SelectedIsDefaultAgentContext
                ));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while handling context menu click");
            }

            // Close with clicky sound
            if (!openedSubmenu)
                addon->AtkUnitBase.FireCallbackInt(-2);
            return false;
        }

        original:
        // Eventually handled by inventorycontext here: 14022BBD0 (6.51)
        return contextMenuOnVf72Hook.Original(addon, selectedIdx, a3);
    }

    public void Dispose()
    {
        contextAddonOpenByAgentHook?.Dispose();
        contextMenuOnVf72Hook?.Dispose();
    }
}

[Addon("ContextMenu")]
[StructLayout(LayoutKind.Explicit, Size = 0x2A0)]
internal unsafe struct AddonContextMenu
{
    [FieldOffset(0x0)] public AtkUnitBase AtkUnitBase;
    [FieldOffset(0x160)] public AtkValue* ContextMenuArray;
    [FieldOffset(0x1CA)] public ushort ContextMenuArrayCount;
}

[Addon("AddonContextMenuTitle", "AddonContextSub")]
[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
internal unsafe struct AddonContextMenuTitle
{
    [FieldOffset(0x0)] public AtkUnitBase AtkUnitBase;
    [FieldOffset(0x160)] public AtkValue* ContextMenuArray;
    [FieldOffset(0x1CA)] public ushort ContextMenuArrayCount;
}

[Agent(AgentId.Context)]
[StructLayout(LayoutKind.Explicit, Size = 0x1750)]
internal unsafe partial struct AgentContext
{
    [FieldOffset(0x00)] public AgentInterface AgentInterface;

    [FieldOffset(0x28)] public fixed byte ContextMenuArray[0x678 * 2];
    [FieldOffset(0x28)] public ContextMenu MainContextMenu;
    [FieldOffset(0x6A0)] public ContextMenu SubContextMenu;
    [FieldOffset(0xD18)] public ContextMenu* CurrentContextMenu;
    [FieldOffset(0xD20)] public Utf8String ContextMenuTitle;
    [FieldOffset(0xD88)] public Point Position;
    [FieldOffset(0xD90)] public uint OwnerAddon;

    [FieldOffset(0xDA0)] public InfoProxyCommonList.CharacterData ContextMenuTarget;
    [FieldOffset(0xE08)] public InfoProxyCommonList.CharacterData* CurrentContextMenuTarget;
    [FieldOffset(0xE10)] public Utf8String TargetName;
    [FieldOffset(0xE78)] public Utf8String YesNoTargetName;

    [FieldOffset(0xEE8)] public ulong TargetContentId;
    [FieldOffset(0xEF0)] public ulong YesNoTargetContentId;
    [FieldOffset(0xEF8)] public GameObjectID TargetObjectId;
    [FieldOffset(0xF00)] public GameObjectID YesNoTargetObjectId;
    [FieldOffset(0xF08)] public short TargetHomeWorldId;
    [FieldOffset(0xF0A)] public short YesNoTargetHomeWorldId;
    [FieldOffset(0xF0C)] public byte YesNoEventId;

    [FieldOffset(0xF10)] public int TargetGender;
    [FieldOffset(0xF14)] public uint TargetMountSeats;

    [FieldOffset(0x1738)] public void* UpdateChecker; // AgentContextUpdateChecker*, if handler returns false the menu closes
    [FieldOffset(0x1740)] public long UpdateCheckerParam; //objectid of the target or list index of an addon or other things
    [FieldOffset(0x1748)] public byte ContextMenuIndex;
    [FieldOffset(0x1749)] public byte OpenAtPosition; // if true menu opens at Position else at cursor location
}

[StructLayout(LayoutKind.Explicit, Size = 0x678)]
internal unsafe partial struct ContextMenu
{
    [FieldOffset(0x00)] public short CurrentEventIndex;
    [FieldOffset(0x02)] public short CurrentEventId;

    [FixedSizeArray<AtkValue>(33)]
    [FieldOffset(0x08)] public fixed byte EventParams[0x10 * 33]; // 32 * AtkValue + 1 * AtkValue for submenus with title

    [FieldOffset(0x428)] public fixed byte EventIdArray[32];
    [FieldOffset(0x450)] public fixed long EventHandlerArray[32];
    [FieldOffset(0x558)] public fixed long EventHandlerParamArray[32];

    [FieldOffset(0x660)] public uint ContextItemDisabledMask;
    [FieldOffset(0x664)] public uint ContextSubMenuMask;
    [FieldOffset(0x668)] public byte* ContextTitleString;
    [FieldOffset(0x670)] public byte SelectedContextItemIndex;
}

[Agent(AgentId.InventoryContext)]
[StructLayout(LayoutKind.Explicit, Size = 0x778)]
internal unsafe partial struct AgentInventoryContext
{
    [FieldOffset(0x0)] public AgentInterface AgentInterface;
    [FieldOffset(0x28)] public uint BlockingAddonId;
    [FieldOffset(0x2C)] public int ContextItemStartIndex;
    [FieldOffset(0x30)] public int ContextItemCount;

    [FieldOffset(0x38)] public fixed byte EventParams[0x10 * 98];
    [FieldOffset(0x658)] public fixed byte EventIdArray[84];
    [FieldOffset(0x6AC)] public uint ContextItemDisabledMask;
    [FieldOffset(0x6B0)] public uint ContextItemSubmenuMask;

    public Span<AtkValue> EventParamsSpan => new(Unsafe.AsPointer(ref EventParams[0]), 98);
    public Span<byte> EventIdSpan => new(Unsafe.AsPointer(ref EventIdArray[0]), 84);

    [FieldOffset(0x6B4)] public int PositionX;
    [FieldOffset(0x6B8)] public int PositionY;

    [FieldOffset(0x6C8)] public uint OwnerAddonId;
    [FieldOffset(0x6CC)] public int YesNoPosition; // 2 shorts combined, gets passed as int arg, default = -1
    [FieldOffset(0x6CC)] public short YesNoX;
    [FieldOffset(0x6CE)] public short YesNoY;
    [FieldOffset(0x6D0)] public InventoryType TargetInventoryId;
    [FieldOffset(0x6D4)] public int TargetInventorySlotId;

    [FieldOffset(0x6DC)] public uint DummyInventoryId;

    [FieldOffset(0x6E8)] public InventoryItem* TargetInventorySlot;
    [FieldOffset(0x6F0)] public InventoryItem TargetDummyItem;
    [FieldOffset(0x728)] public InventoryType BlockedInventoryId;
    [FieldOffset(0x72C)] public int BlockedInventorySlotId;

    [FieldOffset(0x738)] public InventoryItem DiscardDummyItem;
    [FieldOffset(0x770)] public int DialogType; // ?? 1 = Discard, 2 = LowerQuality
}
