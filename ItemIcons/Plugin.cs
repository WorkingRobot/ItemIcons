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

    [Signature("48 8B C4 41 56 48 83 EC 60 FF 81 AC 07 00 00", DetourName = nameof(UIModuleUpdateDetour))]
    private readonly Hook<UIModuleUpdateDelegate> uiModuleUpdateHook = null!;
    private unsafe delegate bool UIModuleUpdateDelegate(UIModule* module, float frameDelta);

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
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenSettingsWindow;

        SignatureHelper.Initialise(this);
        addonSetupHook.Enable();
        addonFinalizeHook.Enable();

        uiModuleUpdateHook.Enable();
    }

    public void Dispose()
    {
        uiModuleUpdateHook?.Dispose();

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

    private unsafe bool UIModuleUpdateDetour(UIModule* module, float frameDelta)
    {
        var ret = uiModuleUpdateHook.Original(module, frameDelta);
        Render();
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

    public void OpenSettingsWindow() =>
        SettingsWindow.IsOpen = true;
}
