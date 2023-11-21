using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace ItemIcons.Utils;

public unsafe class MenuArgs
{
    public nint AgentPtr { get; }
    private bool? IsDefaultContext { get; }

    public GameObjectID ObjectId => DefaultContext->TargetObjectId;

    public ulong ContentId => DefaultContext->TargetContentId;

    public short HomeWorld => DefaultContext->TargetHomeWorldId;

    public InfoProxyCommonList.CharacterData? CharacterData =>
        DefaultContext->CurrentContextMenuTarget != null ?
            *DefaultContext->CurrentContextMenuTarget :
            null;

    public Utf8String Name => DefaultContext->TargetName;

    public InventoryItem Item => *InventoryContext->TargetInventorySlot;

    protected internal MenuArgs(AgentInterface* agent, bool? isDefaultAgentContext)
    {
        AgentPtr = (nint)agent;
        IsDefaultContext = isDefaultAgentContext;
    }

    private AgentContext* DefaultContext =>
        IsDefaultContext == true ?
            (AgentContext*)AgentPtr :
            throw new InvalidOperationException("Not a normal context menu");

    private AgentInventoryContext* InventoryContext =>
        IsDefaultContext == false ?
            (AgentInventoryContext*)AgentPtr :
            throw new InvalidOperationException("Not an inventory menu");
}

public sealed unsafe class MenuItemClickedArgs : MenuArgs
{
    private Action<SeString?, IReadOnlyList<MenuItem>> OnOpenSubmenu { get; }

    internal MenuItemClickedArgs(Action<SeString?, IReadOnlyList<MenuItem>> openSubmenu, AgentInterface* agent, bool? isDefaultAgentContext) : base(agent, isDefaultAgentContext)
    {
        OnOpenSubmenu = openSubmenu;
    }

    public void OpenSubmenu(SeString name, IReadOnlyList<MenuItem> items) =>
        OnOpenSubmenu(name, items);

    public void OpenSubmenu(IReadOnlyList<MenuItem> items) =>
        OnOpenSubmenu(null, items);
}

public sealed unsafe class MenuOpenedArgs : MenuArgs
{
    private Action<MenuItem> OnAddMenuItem { get; }

    internal MenuOpenedArgs(Action<MenuItem> addMenuItem, AgentInterface* agent, bool? isDefaultAgentContext) : base(agent, isDefaultAgentContext)
    {
        OnAddMenuItem = addMenuItem;
    }

    public void AddMenuItem(MenuItem item) =>
        OnAddMenuItem(item);
}

public sealed record MenuItem
{
    public required SeString Name { get; init; }
    public Action<MenuItemClickedArgs>? OnClicked { get; init; }
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
    private unsafe delegate nint RaptureAtkModuleOpenAddonDelegate(RaptureAtkModule* a1, uint addonNameId, uint valueCount, AtkValue* values, AgentInterface* parentAgent, long unk, uint ownerAddonId, int unk2);

    private List<MenuItem> DefaultContextItems { get; } = new();
    private List<MenuItem> InventoryContextItems { get; } = new();

    private AgentInterface* SelectedAgent { get; set; }
    private bool? SelectedIsDefaultContext { get; set; }
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

        return GetPrefixedName(name, SeIconChar.BoxedLetterA + prefix - 'A', colorKey);
    }

    public static SeString GetPrefixedName(SeString name, SeIconChar prefix, ushort colorKey) =>
        new SeStringBuilder()
            .AddUiForeground($"{prefix.ToIconString()} ", colorKey)
            .Append(name)
            .Build();

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

        return (AtkValue*)(newArray + 8);
    }

    private void FreeExpandedContextMenuArray(AtkValue* newValues, int newSize) =>
        IMemorySpace.Free((void*)((nint)newValues - 8), (ulong)((newSize * sizeof(AtkValue)) + 8));

    private AtkValue* CreateEmptySubmenuContextMenuArray(SeString name, int x, int y, out int valueCount)
    {
        // 0: UInt = ContextItemCount
        // 1: String = Name
        // 2: Int = PositionX
        // 3: Int = PositionY
        // 4: Bool = false
        // 5: UInt = ContextItemSubmenuMask
        // 6: UInt = ReturnArrowMask (_gap_0x6BC ? 1 << (ContextItemCount - 1) : 0)
        // 7: UInt = 1

        valueCount = 8;
        var values = ExpandContextMenuArray(Span<AtkValue>.Empty, valueCount);
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

    private void SetupGenericMenu(int headerCount, int sizeHeaderIdx, int returnHeaderIdx, int submenuHeaderIdx, IReadOnlyList<MenuItem> items, ref int valueCount, ref AtkValue* values)
    {
        var itemsWithIdx = items.Select((item, idx) => (item, idx)).OrderBy(i => i.item.Priority);
        var prefixItems = itemsWithIdx.Where(i => i.item.Priority < 0).ToArray();
        var suffixItems = itemsWithIdx.Where(i => i.item.Priority >= 0).ToArray();

        var nativeMenuSize = (int)values[sizeHeaderIdx].UInt;
        var prefixMenuSize = prefixItems.Length;
        var suffixMenuSize = suffixItems.Length;

        var hasGameDisabled = valueCount - headerCount - nativeMenuSize > 0;

        var hasCustomDisabled = items.Any(item => !item.IsEnabled);
        var hasAnyDisabled = hasGameDisabled || hasCustomDisabled;

        values = ExpandContextMenuArray(
            new(values, valueCount),
            valueCount = (nativeMenuSize + items.Count) * (hasAnyDisabled ? 2 : 1) + headerCount
            );
        var offsetData = new Span<AtkValue>(values, headerCount);
        var nameData = new Span<AtkValue>(values + headerCount, nativeMenuSize + items.Count);
        var disabledData = hasAnyDisabled ? new Span<AtkValue>(values + headerCount + nativeMenuSize + items.Count, nativeMenuSize + items.Count) : Span<AtkValue>.Empty;

        var returnMask = offsetData[returnHeaderIdx].UInt;
        var submenuMask = offsetData[submenuHeaderIdx].UInt;

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

        void FillData(Span<AtkValue> disabledData, Span<AtkValue> nameData, int i, MenuItem item, int idx)
        {
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

        for (var i = 0; i < prefixMenuSize; ++i)
        {
            var (item, idx) = prefixItems[i];
            FillData(disabledData, nameData, i, item, idx);
        }

        MenuCallbackIds.AddRange(Enumerable.Range(0, nativeMenuSize).Select(i => -i - 1));

        for (var i = prefixMenuSize + nativeMenuSize; i < prefixMenuSize + nativeMenuSize + suffixMenuSize; ++i)
        {
            var (item, idx) = suffixItems[i - prefixMenuSize - nativeMenuSize];
            FillData(disabledData, nameData, i, item, idx);
        }

        offsetData[returnHeaderIdx].UInt = returnMask;
        offsetData[submenuHeaderIdx].UInt = submenuMask;

        offsetData[sizeHeaderIdx].UInt += (uint)items.Count;
    }

    private void SetupContextMenu(IReadOnlyList<MenuItem> items, ref int valueCount, ref AtkValue* values)
    {
        // 0: UInt = Item Count
        // 1: UInt = 0 (probably window name, just unused)
        // 2: UInt = Return Mask (?)
        // 3: UInt = Submenu Mask
        // 4: UInt = OpenAtCursorPosition ? 2 : 1
        // 5: UInt = 0
        // 6: UInt = 0

        SetupGenericMenu(7, 0, 2, 3, items, ref valueCount, ref values);
    }

    private void SetupContextSubMenu(IReadOnlyList<MenuItem> items, ref int valueCount, ref AtkValue* values)
    {
        // 0: UInt = ContextItemCount
        // 1: skipped?
        // 2: Int = PositionX
        // 3: Int = PositionY
        // 4: Bool = false
        // 5: UInt = ContextItemSubmenuMask
        // 6: UInt = _gap_0x6BC ? 1 << (ContextItemCount - 1) : 0
        // 7: UInt = 1

        SetupGenericMenu(8, 0, 5, 6, items, ref valueCount, ref values);
    }

    private nint ContextAddonOpenByAgentDetour(RaptureAtkModule* module, byte* addonName, AtkUnitBase* addon, int valueCount, AtkValue* values, AgentInterface* agent, nint a7, ushort a8)
    {
        var oldValues = values;

        var n = MemoryHelper.ReadStringNullTerminated((nint)addonName);
        if (n.Equals("ContextMenu", StringComparison.Ordinal))
        {
            MenuCallbackIds.Clear();
            SelectedAgent = agent;
            if (SelectedAgent == AgentInventoryContext.Instance())
            {
                SelectedItems = InventoryContextItems;
                SelectedIsDefaultContext = false;
            }
            else if (SelectedAgent == AgentContext.Instance())
            {
                SelectedItems = DefaultContextItems;
                SelectedIsDefaultContext = true;
            }
            else
            {
                SelectedItems = null;
                SelectedIsDefaultContext = null;
            }
            SubmenuItems = null;

            if (SelectedItems != null)
            {
                SelectedItems = new(SelectedItems);
                OnMenuOpened?.Invoke(new(SelectedItems.Add, SelectedAgent, SelectedIsDefaultContext));
                SetupContextMenu(SelectedItems, ref valueCount, ref values);
            }
        }
        else if (n.Equals("AddonContextSub", StringComparison.Ordinal))
        {
            MenuCallbackIds.Clear();
            if (SubmenuItems != null)
                SetupContextSubMenu(SubmenuItems, ref valueCount, ref values);
        }

        var ret = contextAddonOpenByAgentHook.Original(module, addonName, addon, valueCount, values, agent, a7, a8);
        if (values != oldValues)
            FreeExpandedContextMenuArray(values, valueCount);
        return ret;
    }

    private void OpenSubmenu(SeString name, IReadOnlyList<MenuItem> submenuItems, int posX, int posY)
    {
        if (submenuItems.Count == 0)
            throw new ArgumentException("Submenu must not be empty", nameof(submenuItems));

        SubmenuItems = submenuItems;

        var module = RaptureAtkModule.Instance();
        var values = CreateEmptySubmenuContextMenuArray(name, posX, posY, out var valueCount);
        uint ownerAddonId;

        if (SelectedIsDefaultContext == true)
        {
            ownerAddonId = ((AgentContext*)SelectedAgent)->OwnerAddon;
            raptureAtkModuleOpenAddon(module, 445, (uint)valueCount, values, SelectedAgent, 71, ownerAddonId, 4);
        }
        else if (SelectedIsDefaultContext == false)
        {
            ownerAddonId = ((AgentInventoryContext*)SelectedAgent)->OwnerAddonId;
            raptureAtkModuleOpenAddon(module, 445, (uint)valueCount, values, SelectedAgent, 0, ownerAddonId, 4);
        }
        else
            Log.Debug($"Unknown agent {(nint)SelectedAgent:X8}");

        FreeExpandedContextMenuArray(values, valueCount);
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
                if (item.OnClicked == null)
                    throw new InvalidOperationException("Item has no OnClicked handler");
                item.OnClicked(new(
                    (name, items) =>
                    {
                        short x, y;
                        addon->AtkUnitBase.GetPosition(&x, &y);
                        OpenSubmenu(name ?? item.Name, items, x, y);
                        openedSubmenu = true;
                    },
                    SelectedAgent,
                    SelectedIsDefaultContext
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
