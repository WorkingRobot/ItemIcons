using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Utility.Signatures;
using ItemIcons.Windows;
using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using ItemIcons.Utils;
using System.Reflection;
using ItemIcons.IconProviders;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace ItemIcons;

public sealed class Plugin : IDalamudPlugin
{
    public WindowSystem WindowSystem { get; }

    private Settings SettingsWindow { get; }
    public IconRenderer Renderer { get; }
    public IconManager IconManager { get; }
    private CtxMenu ContextMenu { get; }

    [Signature("E8 ?? ?? ?? ?? 8B 83 ?? ?? ?? ?? C1 E8 14", DetourName = nameof(AddonSetupDetour))]
    private readonly Hook<AddonSetupDelegate> addonSetupHook = null!;
    [Signature("E8 ?? ?? ?? ?? 48 8B 7C 24 ?? 41 8B C6", DetourName = nameof(AddonFinalizeDetour))]
    private readonly Hook<AddonFinalizeDelegate> addonFinalizeHook = null!;

    private unsafe delegate void* AddonSetupDelegate(AtkUnitBase* addon);
    private unsafe delegate void AddonFinalizeDelegate(AtkUnitManager* manager, AtkUnitBase** addon);

    // Client::UI::UIModule.Update
    [Signature("48 8B C4 41 56 48 83 EC 60 FF 81 E4 07 00 00", DetourName = nameof(UIModuleUpdateDetour))]
    private readonly Hook<UIModuleUpdateDelegate> uiModuleUpdateHook = null!;
    private unsafe delegate bool UIModuleUpdateDelegate(UIModule* module, float frameDelta);

    [Signature("E8 ?? ?? ?? ?? 48 8B 45 28 48 8B CE")]
    public readonly AtkUldManagerUpdateNodeTreeDelegate atkUldManagerUpdateNodeTree = null!;
    public unsafe delegate void AtkUldManagerUpdateNodeTreeDelegate(AtkUldManager* manager, AtkResNode* node, AtkResNode* nodeParent, bool isShallowUpdate);

    public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        Service.Plugin = this;
        pluginInterface.Create<Service>();
        try
        {
            Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new();
        }
        catch (TargetInvocationException ex)
        {
            Log.Error(ex.InnerException!, "Error while loading config. Using default config.");
            Service.Configuration = new();
        }
        
        WindowSystem = new();

        Renderer = new();
        SettingsWindow = new();
        IconManager = new();

        ContextMenu = new();
        ContextMenu.OnMenuOpened += OnMenuOpened;

        Service.CommandManager.AddHandler("/ii", new((_, _) => OpenSettingsWindow()) { HelpMessage = "Open the Item Icons settings window." });

        Service.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenSettingsWindow;

        Service.GameInteropProvider.InitializeFromAttributes(this);
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
        
        Renderer.Dispose();
        IconManager.Dispose();
        ContextMenu.Dispose();
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
            Log.Error(ex, "Error while drawing icons");
        }
    }

    private void OnMenuOpened(MenuOpenedArgs args)
    {
        if (!args.IsInventory)
            return;

        if (!Service.Configuration.IsIconProviderEnabled(typeof(Favorites)))
            return;

        var item = Item.FromInventoryItem(args._Item)!.Value;
        if (item.ItemId == 0)
            return;

        args.AddMenuItem(new()
        {
            Name = CtxMenu.GetPrefixedName("Favorite", 'I', 541),
            OnClicked = OpenFavoriteSubmenu,
            IsSubmenu = true,
        });
    }

    private void OpenFavoriteSubmenu(MenuItemClickedArgs args)
    {
        var items = new List<MenuItem>();

        var item = Item.FromInventoryItem(args._Item)!.Value;

        var hasId = Service.Configuration.FavoritedItems.TryGetValue(item.ItemId, out var existingId);
        BitmapFontIcon? favoritedIcon = hasId ? (existingId < Favorites.IconIds.Length ? Favorites.IconIds[existingId] : null) : null;

        uint itemIdx = 0;
        foreach(var icon in Favorites.IconIds)
        {
            var b = new SeStringBuilder();
            b.AddIcon(icon);
            if (favoritedIcon == icon)
                b.AddUiGlow($" (Remove)", 14);
            var idx = itemIdx;
            items.Add(new()
            {
                Name = b.Build(),
                OnClicked = _ => SetFavoriteForItem(item.ItemId, idx),
            });
            itemIdx++;
        }

        args.OpenSubmenu("Favorite", items);
    }

    private void SetFavoriteForItem(uint itemId, uint itemIdx)
    {
        if (Service.Configuration.FavoritedItems.TryGetValue(itemId, out var existingId) && existingId == itemIdx)
            Service.Configuration.FavoritedItems.Remove(itemId);
        else
            Service.Configuration.FavoritedItems[itemId] = itemIdx;
        Service.Configuration.Save();
    }

    public void OpenSettingsWindow() =>
        SettingsWindow.IsOpen = true;
}
