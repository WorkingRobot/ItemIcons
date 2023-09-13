using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Utility.Signatures;
using ItemIcons.Windows;
using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace ItemIcons;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Item Icons";

    public WindowSystem WindowSystem { get; }

    private Settings SettingsWindow { get; }
    public IconRenderer Renderer { get; }

    [Signature("E8 ?? ?? ?? ?? 8B 83 ?? ?? ?? ?? C1 E8 14", DetourName = nameof(AddonSetupDetour))]
    private readonly Hook<AddonSetupDelegate> addonSetupHook = null!;
    [Signature("E8 ?? ?? ?? ?? 48 8B 7C 24 ?? 41 8B C6", DetourName = nameof(AddonFinalizeDetour))]
    private readonly Hook<AddonFinalizeDelegate> addonFinalizeHook = null!;

    private unsafe delegate void* AddonSetupDelegate(AtkUnitBase* addon);
    private unsafe delegate void AddonFinalizeDelegate(AtkUnitManager* manager, AtkUnitBase** addon);

    //[StructLayout(LayoutKind.Explicit, Size = 9)]
    //private struct ComponentLoadIconData
    //{
    //    [FieldOffset(0)]
    //    public int IconId;
    //    [FieldOffset(4)]
    //    public int Data;
    //    [FieldOffset(8)]
    //    public bool IsValid;
    //}
    //[Signature("48 89 5C 24 08 57 48 83 EC 20 48 8B D9 48 8B FA 48 8B 89 C8 00 00 00 48 85 C9 ?? ?? ?? ?? ?? ?? 80 7A 08 00 ?? ?? 8B 02 39 83 C0 00 00 00", DetourName = nameof(ComponentLoadIconDetour))]
    //private readonly Hook<ComponentLoadIconDelegate> componentLoadIconHook = null!;
    //private unsafe delegate bool ComponentLoadIconDelegate(AtkComponentIcon* icon, ComponentLoadIconData* data);

    //[Signature("4C 8B DC 53 56 48 81 EC C8 02 00 00 ?? ?? ?? ?? ?? ?? ?? 48 33 C4 48 89 84 24 90 02 00 00 48 83 B9 60 2B 00 00 00", DetourName = nameof(TaskUpdateInputUIDetour))]
    //private readonly Hook<TaskUpdateInputUIDelegate> taskUpdateInputUIHook = null!;
    //private unsafe delegate void TaskUpdateInputUIDelegate(void* a1, void* a2);

    //// 4C 8B DC 53 56 41 54 41 56 48 81 EC ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 48 33 C4 ?? ?? ?? ?? ?? ?? ?? ?? 49 89 6B 18 48 8B F1 ?? ?? ?? ?? ?? ?? ?? 49 89 7B D8 4D 89 7B C8 45 32 FF
    //// Component::GUI::AtkUnitManager_vf5
    //[Signature("4C 8B DC 53 56 41 54 41 56 48 81 EC", DetourName = nameof(AtkUnitManagerUpdateDetour))]
    //private readonly Hook<AtkUnitManagerUpdateDelegate> atkUnitManagerUpdateHook = null!;
    //private unsafe delegate nint AtkUnitManagerUpdateDelegate(AtkUnitManager* a1);

    // AtkModule_Draw2DBegin
    //[Signature("40 53 48 83 EC 20 80 B9 ?? ?? 00 00 00 48 8B D9 74 ?? 48 8B 89 30 01 00 00 48 89 7C 24 38 48 85 C9", DetourName = nameof(AtkModuleDrawDetour))]
    //private readonly Hook<AtkModuleDrawDelegate> atkModuleDrawHook = null!;
    //private unsafe delegate void AtkModuleDrawDelegate(AtkModule* a1);

    // Close as I can get? I'm not sure. The only issue with this one is that switching inventory tabs has a 1 frame delay
    // But drag & drop works fine. Maybe inventory stuff is handled later in the frame, but before the draw.
    //[Signature("40 56 48 83 EC 40 80 79 1A 00 48 8B F1", DetourName = nameof(HandleInputUpdateDetour))]
    //private readonly Hook<HandleInputUpdateDelegate> handleInputUpdateHook = null!;
    //private unsafe delegate bool HandleInputUpdateDelegate(nint uiInputModule);

    //[Signature("48 89 5C 24 08 57 48 83 EC 20 41 0F B6 F8 48 8B D9 41 80 F8 01 75 79 33 C0 F0 0F C0 81 A8 00 00 00 3C 03 74 6B 80 3A 0B 75 29 83 BA 84 00 00 00 02", DetourName = nameof(TextureResourceHandleLoadDetour))]
    //private readonly Hook<TextureResourceHandleLoadDelegate> textureResourceHandleLoadHook = null!;
    //private unsafe delegate bool TextureResourceHandleLoadDelegate(TextureResourceHandle* handle, nint a2, bool a3);

    //[Signature("E8 ?? ?? ?? ?? 4C 8B 6C 24 ?? 4C 8B 5C 24 ?? ??", DetourName = nameof(AtkTextureLoadTextureDetour))]
    //private readonly Hook<AtkTextureLoadTextureDelegate> atkTextureLoadTextureHook = null!;
    //private unsafe delegate int AtkTextureLoadTextureDelegate(AtkTexture* texture, byte* path, int version);

    //[Signature("40 53 48 83 EC 40 41 83 F8 01", DetourName = nameof(FormatIconPathDetour))]
    //private readonly Hook<FormatIconPathDelegate> formatIconPathHook = null!;
    //private unsafe delegate int FormatIconPathDelegate(byte* path, int iconId, int isHq, int version);

    [Signature("48 8B C4 41 56 48 83 EC 60 FF 81 AC 07 00 00", DetourName = nameof(UIModuleUpdateDetour))]
    private readonly Hook<UIModuleUpdateDelegate> uiModuleUpdateHook = null!;
    private unsafe delegate bool UIModuleUpdateDelegate(UIModule* module, float frameDelta);

    //[Signature("48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 48 83 EC 30 41 0F B6 F1 49 8B D8 48 8B FA")]
    //public readonly AtkUldManagerResolveDirtyDelegate atkUldManagerResolveDirty = null!;
    //public unsafe delegate void AtkUldManagerResolveDirtyDelegate(AtkUldManager* manager, AtkResNode* node, AtkResNode* nodeParent, bool dontForceDirty);

    [Signature("48 89 5C 24 10 57 48 83 EC 20 8B 82 A0 00 00 00")]
    public readonly AtkUldManagerUpdateNodeTransformDelegate atkUldManagerUpdateNodeTransform = null!;
    public unsafe delegate void AtkUldManagerUpdateNodeTransformDelegate(AtkUldManager* manager, AtkResNode* node, AtkResNode* nodeParent);

    [Signature("4C 8B CA 4D 85 C0 75 43")]
    public readonly AtkUldManagerUpdateNodeColorsDelegate atkUldManagerUpdateNodeColors = null!;
    public unsafe delegate void AtkUldManagerUpdateNodeColorsDelegate(AtkUldManager* manager, AtkResNode* node, AtkResNode* nodeParent);

    public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        Service.Plugin = this;
        pluginInterface.Create<Service>();
        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new();
        Service.CommandManager.AddHandler("/ii", new((a, b) => OpenSettingsWindow()) { HelpMessage = "Open the Item Icons settings window." });
        
        Renderer = new();

        WindowSystem = new();
        SettingsWindow = new();

        Service.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        //Service.PluginInterface.UiBuilder.Draw += Render;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenSettingsWindow;

        SignatureHelper.Initialise(this);
        addonSetupHook.Enable();
        addonFinalizeHook.Enable();

        //taskUpdateInputUIHook.Enable();
        //atkUnitManagerUpdateHook.Enable();
        //atkModuleDrawHook.Enable();
        //handleInputUpdateHook.Enable();
        //textureResourceHandleLoadHook.Enable();
        //atkTextureLoadTextureHook.Enable();
        //formatIconPathHook.Enable();
        uiModuleUpdateHook.Enable();
    }

    public void Dispose()
    {
        uiModuleUpdateHook?.Dispose();
        //formatIconPathHook?.Dispose();
        //atkTextureLoadTextureHook?.Dispose();
        //textureResourceHandleLoadHook?.Dispose();
        //handleInputUpdateHook?.Dispose();
        //atkModuleDrawHook?.Dispose();
        //atkUnitManagerUpdateHook?.Dispose();
        //taskUpdateInputUIHook?.Dispose();

        addonSetupHook?.Dispose();
        addonFinalizeHook?.Dispose();

        Service.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        Service.CommandManager.RemoveHandler("/ii");

        WindowSystem.RemoveAllWindows();
        //IconProvider.DisposeRegistry(); Causes crashes atm
        Renderer.Dispose();
    }

    private unsafe void* AddonSetupDetour(AtkUnitBase* addon)
    {
        var ret = addonSetupHook.Original(addon);
        Renderer.SetupAddon(addon);
        return ret;
    }

    private unsafe void AddonFinalizeDetour(AtkUnitManager* manager, AtkUnitBase** addonPtr)
    {
        Renderer.FinalizeAddon(*addonPtr);
        addonFinalizeHook.Original(manager, addonPtr);
    }

    //private unsafe void TaskUpdateInputUIDetour(void* a1, void* a2)
    //{
    //    taskUpdateInputUIHook.Original(a1, a2);
    //    //LogInvTab("Task Input");
    //    Render();
    //}

    //private unsafe void AtkModuleDrawDetour(AtkModule* a1)
    //{
    //    Render();
    //    atkModuleDrawHook.Original(a1);
    //    //LogInvTab("Module Draw");
    //}

    //private unsafe nint AtkUnitManagerUpdateDetour(AtkUnitManager* a1)
    //{
    //    var ret = atkUnitManagerUpdateHook.Original(a1);
    //    //LogInvTab("Manager Update");
    //    //try
    //    //{
    //    //    Renderer.Draw();
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    PluginLog.Error(ex, "Error while drawing icons");
    //    //}
    //    return ret;
    //}

    //private bool HandleInputUpdateDetour(nint uiInputModule)
    //{
    //    var ret = handleInputUpdateHook.Original(uiInputModule);
    //    Render();
    //    //LogInvTab("Input Update");
    //    return ret;
    //}

    private unsafe bool UIModuleUpdateDetour(UIModule* module, float frameDelta)
    {
        var ret = uiModuleUpdateHook.Original(module, frameDelta);
        Render();
        //LogInvTab("Input Update");
        return ret;
    }

    private void Render()
    {
        try
        {
            Renderer.Draw();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Error while drawing icons");
        }
    }

    //private unsafe bool TextureResourceHandleLoadDetour(TextureResourceHandle* handle, nint a2, bool a3)
    //{
    //    var ret = textureResourceHandleLoadHook.Original(handle, a2, a3);
    //    PluginLog.Debug($"Texture Load: {(nint)handle:X8} {handle->ResourceHandle.Category} {handle->ResourceHandle.FileName} {a2:X8} {a3} -> {ret}");
    //    return ret;
    //}

    //private unsafe int AtkTextureLoadTextureDetour(AtkTexture* texture, byte* path, int version)
    //{
    //    PluginLog.Debug($"Texture Load: {(nint)texture:X8} {MemoryHelper.ReadStringNullTerminated((nint)path)} {version}");
    //    return atkTextureLoadTextureHook.Original(texture, path, version);
    //}

    //private unsafe int FormatIconPathDetour(byte* path, int iconId, int isHq, int version)
    //{
    //    var ret = formatIconPathHook.Original(path, iconId, isHq, version);
    //    PluginLog.Debug($"Format Icon Path: {MemoryHelper.ReadStringNullTerminated((nint)path)} {iconId} {isHq} {version}");
    //    return ret;
    //}

    //private static void LogInvTab(string name) =>
    //    PluginLog.Debug($"[{name}] => {GetInvTab()}");

    //private static unsafe int GetInvTab()
    //{
    //    var addon = Service.GameGui.GetAddonByName("InventoryLarge");
    //    if (addon == nint.Zero)
    //        return -1;
    //    var addonPtr = (AtkUnitBase*)addon;
    //    if (addonPtr->RootNode == null)
    //        return -2;
    //    if (!addonPtr->RootNode->IsVisible)
    //        return -3;
    //    return GetInvTab(addon);
    //}

    //private static unsafe int GetInvTab(nint addon)
    //{
    //    var board = (AtkUnitBase*)addon;
    //    for (var i = 0; i < 2; ++i)
    //    {
    //        var id = (uint)i + 7;
    //        if (NodeUtils.GetNodeById(&board->GetNodeById(id)->GetAsAtkComponentRadioButton()->AtkComponentBase, 3)->GetAsAtkNineGridNode()->AtkResNode.IsVisible)
    //            return i;
    //    }
    //    return -4;
    //}

    public void OpenSettingsWindow() =>
        SettingsWindow.IsOpen = true;
}
